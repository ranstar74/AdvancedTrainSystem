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
        /// Previous frame <see cref="Speed"/>.
        /// </summary>
        private float _prevSpeed;

        private float _throttle;
        /// <summary>
        /// How much throttle is opened. 0 is closed, 1 is fully opened.
        /// </summary>
        public float Throttle
        {
            get => _throttle;
            set
            {
                _throttle = value.Clamp(0, 1);
            }
        }

        private float _gear;
        /// <summary>
        /// Gear. Also known as Johnson bar. 1 forward, -1 backward
        /// </summary>
        public float Gear
        {
            get => _gear;
            set
            {
                _gear = value.Clamp(-1, 1);
            }
        }

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
            if(Base.CollisionComponent.IsDerailed)
            {
                Speed = 0;
                return;
            }
            
            // TODO: Take uphill/downhill into account

            // Acceleration = (v1 - v2) / t
            float acceleration = (Speed - _prevSpeed) * Game.LastFrameTime;

            _prevSpeed = Speed;

            float velocty = Entity.Velocity.Length();
            float airBrakeInput = Base.BrakeComponent.AirbrakeForce;
            float steamBrakeInput = 1 - Base.BrakeComponent.SteamBrake;
            float boilerPressure = Base.BoilerComponent.Pressure.Remap(0, 300, 0, 1);

            // Calculate forces

            // Wheel traction - too fast acceleration will cause bad traction
            float wheelTraction = Math.Abs(Speed).Remap(2, 0, 0, 40).Remap(0, 40, 0, 18);
            wheelTraction *= ((float)Math.Pow(Throttle, 10)).Remap(0, 1, 0, 2);
            if (Speed > 10 || wheelTraction < 1)
                wheelTraction = 1;

            // Surface resistance force - wet surface increases resistance
            float surfaceResistance = RainPuddleEditor.Level + 1;

            float wheelRatio = (Speed.Remap(0, 40, 40, 0) + 0.01f) / (DriveWheelSpeed + 0.01f);
            wheelRatio = wheelRatio / (150 * surfaceResistance.Remap(1, 2, 1, 1.3f)) + 1;

            // Friction force = 0.2 * speed * difference between wheel and train speed
            float frictionForce = 0.2f * Speed / 2 * wheelRatio;

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

            // Combine all forces
            float totalForce = (steamForce * brakeMultiplier * steamBrakeInput) - dragForce + inerciaForce - frictionForce - brakeForce;
            totalForce *= AccelerationMultiplier * Game.LastFrameTime;

            Speed += totalForce;

            // We making it non directional because wheel and speed direction doesn't always match
            float baseWheelSpeed = Math.Abs(Speed);

            DriveWheelSpeed = baseWheelSpeed * wheelTraction * steamBrakeInput * forceDirection;

            // Set speed
            Base.Speed = Speed;

            // Check if train is accelerating
            IsTrainAccelerating = Math.Abs(steamForce) > 0;

            // Check if wheel are sparking
            AreWheelSpark = wheelTraction > 5 || (steamBrakeInput == 0 && baseWheelSpeed > 1.5f);

            // Invoke OnTrainStart
            var absSpeed = Math.Abs(Speed);
            if (absSpeed > 0.3f && absSpeed < 4f && IsTrainAccelerating)
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
        }
    }
}
