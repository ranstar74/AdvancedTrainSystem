using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using System;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Calculates speed of the train.
    /// </summary>
    public class SpeedComponent : Component<CustomTrain>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Entity Entity { get; set; }

        /// <summary>
        /// Speed of the train.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Absolute speed of the train.
        /// </summary>
        public float AbsoluteSpeed {  get; private set; }

        /// <summary>
        /// Absolute value of speed difference between this frame and last frame.
        /// </summary>
        public float LastFrameAcceleration => Math.Abs(Speed - _prevSpeed);

        /// <summary>
        /// Previous frame <see cref="Speed"/>.
        /// </summary>
        private float _prevSpeed;

        /// <summary>
        /// How much throttle is opened. 0 is closed, 1 is fully opened.
        /// </summary>
        public float Throttle { get; internal set; }

        /// <summary>
        /// Gear. Also known as Johnson bar. 1 forward, -1 backward
        /// </summary>
        public float Gear { get; internal set; }

        /// <summary>
        /// How fast train accelerates.
        /// </summary>
        public float AccelerationMultiplier = 0.2f;

        /// <summary>
        /// Returns True if drive wheel are sparking, otherwise False.
        /// </summary>
        public bool AreWheelSpark { get; private set; }

        private bool _onTrainStartInvoked = false;
        /// <summary>
        /// Invokes when train started moving.
        /// </summary>
        public Action OnTrainStart { get; set; }

        /// <summary>
        /// Whether train is accelerating or not.
        /// </summary>
        public bool IsTrainAccelerating { get; private set; }

        /// <summary>
        /// Drive wheels speed. Could be higher than train speed if train slips.
        /// </summary>
        public float DriveWheelSpeed { get; private set; }

        /// <summary>
        /// Last forces that were applied on train.
        /// </summary>
        public float LastForces { get; private set; }

        /// <summary>
        /// <see cref="LastForces"/> of previous frame.
        /// </summary>
        public float PreviousLastForces { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Start()
        {
            ((Vehicle)Entity).SetTrainCruiseSpeed(0);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {
            // Since invisible vehicle aren't used after derail we just set speed on zero
            if (Base.DerailComponent.IsDerailed)
            {
                Speed = 0;
                return;
            }

            Speed += CalculateForces();

            // Set speed
            _prevSpeed = Base.Speed;
            Base.Speed = Speed;
        }

        /// <summary>
        /// Calculates all train forces.
        /// </summary>
        /// <returns>All train forces of this frame.</returns>
        private float CalculateForces()
        {
            // TODO: Take uphill/downhill into account

            // Acceleration = (v1 - v2) / t
            float acceleration = LastFrameAcceleration * Game.LastFrameTime;

            float velocty = Entity.Velocity.Length();
            float airBrakeInput = Base.BrakeComponent.AirbrakeForce;
            float steamBrakeInput = 1 - Base.BrakeComponent.FullBrakeForce;
            float boilerPressure = Base.BoilerComponent.Pressure.Remap(0, 300, 0, 1);
            AbsoluteSpeed = Math.Abs(Speed);

            // Calculate forces

            // Wheel traction - too fast acceleration will cause bad traction
            float wheelTraction = Math.Abs(Speed).Remap(2, 0, 0, 40).Remap(0, 40, 0, 18);
            wheelTraction *= ((float)Math.Pow(Throttle, 10)).Remap(0, 1, 0, 2);
            if (Speed > 10 || wheelTraction < 1)
                wheelTraction = 1;

            // Surface resistance force - wet surface increases resistance
            float surfaceResistance = RainPuddleEditor.Level + 1;

            float wheelRatio = (AbsoluteSpeed.Remap(0, 40, 40, 0) + 0.01f) / (Math.Abs(DriveWheelSpeed) + 0.01f);
            wheelRatio = wheelRatio / (150 * surfaceResistance.Remap(1, 2, 1, 1.3f)) + 1;

            // Friction force = 0.2 * speed * difference between wheel and train speed
            float frictionForce = 0.2f * AbsoluteSpeed / 2 * wheelRatio;

            // Brake force
            float brakeForce = Speed * airBrakeInput * 2;

            // Air resistance force = 0.02 * velocty^2
            float dragForce = (float)(0.02f * Math.Pow(velocty, 2)) / 8;

            // Inercia force = acceleration * mass
            float inerciaForce = acceleration * 5;

            // How much steam going into cylinder
            float steamForce = Throttle.Remap(0, 1, 0, 4) * Gear * boilerPressure;

            // Direction of force
            float forceFactor = Throttle <= 0.1f || Math.Abs(Gear) <= 0.1f ? Speed : Gear;
            int forceDirection = forceFactor >= 0 ? 1 : -1;

            // Brake multiplier
            float brakeMultiplier = airBrakeInput.Remap(0, 1, 1, 0);

            float totalResistanceForces = dragForce + inerciaForce + frictionForce;
            if (Speed < 0)
                totalResistanceForces *= -1;
            // Combine all forces
            float totalForce = 
                (steamForce * brakeMultiplier * steamBrakeInput) - brakeForce - totalResistanceForces;
            totalForce *= AccelerationMultiplier * Game.LastFrameTime;

            // We making it non directional because wheel and speed direction doesn't always match
            var driveWheelSpeed = AbsoluteSpeed * wheelTraction * steamBrakeInput * forceDirection;

            // For some reason TrainSetSpeed function cut any speed below about 0.15,
            // we don't want wheels to spin when train is still
            // Its probably was implemented as "hack" to stop train
            if (AbsoluteSpeed < 0.15f)
                driveWheelSpeed = 0;

            DriveWheelSpeed = driveWheelSpeed;

            GTA.UI.Screen.ShowSubtitle($"S {Speed} AS {AbsoluteSpeed} WT {wheelTraction} DS {DriveWheelSpeed}");

            // Check if train is accelerating
            IsTrainAccelerating = Math.Abs(steamForce) > 0;

            // Check if wheel are sparking
            AreWheelSpark = wheelTraction > 5 && AbsoluteSpeed > 0.1f || (steamBrakeInput == 0 && AbsoluteSpeed > 1.5f);

            // Invoke OnTrainStart
            if (AbsoluteSpeed > 0.3f && AbsoluteSpeed < 4f && IsTrainAccelerating)
            {
                if (!_onTrainStartInvoked)
                {
                    OnTrainStart?.Invoke();
                    _onTrainStartInvoked = true;
                }
            }
            else
            {
                _onTrainStartInvoked = false;
            }

            //GTA.UI.Screen.ShowSubtitle(
            //    $"F: {frictionForce.ToString("0.00")} " +
            //    $"D:{dragForce.ToString("0.00")} " +
            //    $"I: {inerciaForce.ToString("0.00")} " +
            //    $"S: {steamForce.ToString("0.00")} " +
            //    $"T: {totalForce.ToString("0.00")} " +
            //    $"FD: {forceDirection}" + 
            //    $"TR: {totalResistanceForces}");

            PreviousLastForces = LastForces;
            LastForces = totalForce; 
            return totalForce;
        }
        
        /// <summary>
        /// Applies some external force on train.
        /// </summary>
        public void ApplyForce(float force)
        {
            Speed += force;
        }
    }
}
