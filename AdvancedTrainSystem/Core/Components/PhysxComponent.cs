using AdvancedTrainSystem.Core.Components.Physics;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>
    /// Calculates speed of the train.
    /// </summary>
    public class PhysxComponent : Component
    {
        private float speed;
        /// <summary>
        /// Speed of the train.
        /// </summary>
        public float Speed
        {
            get => speed;
            set
            {
                speed = value;

                // For some goddamn reason train speed in gta 
                // isnt equals the one u've set, its a bit lower
                // So no matter what speed u've set it never gonna 
                // match on two trains and will result offset after some time
                // But cruise speed is much closer to speed u've set 
                // so we can use it when two trains are coupled/pushing each other
                // Totally:
                // TrainSpeed
                // - Bad precision
                // - Fast responce (whatever speed you set it instantly sets)
                // CruiseSpeed
                // - Better precision (still far from perfect)
                // - Slow as hell responce, it takes seconds to reach speed u've set
                if (collision.IsTrainCoupled && (Game.FrameCount - collision.CoupleFrame) > 5)
                {
                    train.TrainLocomotive.HiddenVehicle.SetTrainCruiseSpeed(speed);
                }
                else
                {
                    train.TrainLocomotive.HiddenVehicle.SetTrainCruiseSpeed(0);
                    train.TrainLocomotive.HiddenVehicle.SetTrainSpeed(speed);
                }
            }
        }

        /// <summary>
        /// Track speed is not depends on train direction. 
        /// Can be used if u want to move two trains with different direction with same direction.
        /// </summary>
        public float TrackSpeed
        {
            get => train.Direction ? Speed : -Speed;
            set
            {
                if (train.Direction)
                    Speed = value;
                else
                    Speed = -value;
            }
        }

        /// <summary>
        /// Absolute speed of the train.
        /// </summary>
        public float AbsoluteSpeed {  get; private set; }

        /// <summary>
        /// Absolute value of speed difference between this frame and last frame.
        /// </summary>
        public float AbsoluteLastFrameAcceleration { get; private set; }

        /// <summary>
        /// Speed difference between this frame and last frame.
        /// </summary>
        public float LastFrameAcceleration { get; private set; }

        /// <summary>
        /// Previous frame <see cref="Speed"/>.
        /// </summary>
        public float PreviousSpeed;

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
        /// How much wheel slip in range of 0.0 - 1.0
        /// </summary>
        public float WheelSlipFactor { get; private set; }

        /// <summary>
        /// <see cref="LastForces"/> of previous frame.
        /// </summary>
        public float PreviousLastForces { get; set; }

        /// <summary>
        /// Pending move actions.
        /// </summary>
        private readonly List<(float, int)> moveStack = new List<(float, int)>();

        private readonly Train train;
        private DerailComponent derail;
        private CollisionComponent collision;

        public PhysxComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            derail = Components.GetComponent<DerailComponent>();
            collision = Components.GetComponent<CollisionComponent>();
        }

        public override void Update()
        {
            // Since invisible vehicle aren't used after derail we just set speed on zero
            if (derail.IsDerailed)
            {
                Speed = 0;
                return;
            }

            ProcessMove();

            PreviousSpeed = Speed;

            Speed += CalculateForces();

            AbsoluteSpeed = Math.Abs(Speed);
            LastFrameAcceleration = Speed - PreviousSpeed;
            AbsoluteLastFrameAcceleration = Math.Abs(LastFrameAcceleration);

            TrainCollisionSolver.Update();
        }

        /// <summary>
        /// Processes all pending move actions of <see cref="Move(float)"/>
        /// </summary>
        private void ProcessMove()
        {
            var movingPoolToRemove = new List<(float, int)>();
            for (int i = 0; i < moveStack.Count; i++)
            {
                (float distance, int tick) = moveStack[i];

                if (tick == Game.FrameCount)
                    continue;

                Speed -= distance;
                movingPoolToRemove.Add((distance, tick));
            }

            for (int i = 0; i < movingPoolToRemove.Count; i++)
            {
                moveStack.Remove(movingPoolToRemove[i]);
            }
        }

        /// <summary>
        /// Calculates all train forces.
        /// </summary>
        /// <returns>All train forces of this frame.</returns>
        private float CalculateForces()
        {
            // TODO: Take uphill/downhill into account

            // Acceleration = (v1 - v2) / t
            float acceleration = AbsoluteLastFrameAcceleration * Game.LastFrameTime;

            float velocty = train.Velocity.Length();

            // TODO: FIX UNASSIGNED VALUES
            float airBrakeInput = 0;//Parent.Components.BrakeComponent.AirbrakeForce;
            float steamBrakeInput = 0;//1 - Parent.Components.BrakeComponent.FullBrakeForce;
            float boilerPressure = 0;//Parent.Components.BoilerComponent.Pressure.Remap(0, 300, 0, 1);

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

            if (AbsoluteSpeed > 0.25f)
                WheelSlipFactor = wheelTraction.Remap(0, 18, 0, 1).Clamp(0, 1);
            else
                WheelSlipFactor = 0f;

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

            //GTA.UI.Screen.ShowSubtitle($"S {Speed} AS {AbsoluteSpeed} WT {wheelTraction} DS {DriveWheelSpeed}");

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

        /// <summary>
        /// Applies some external force on train on track speed.
        /// </summary>
        public void ApplyTrackForce(float force)
        {
            TrackSpeed += force;
        }

        /// <summary>
        /// Moves train on specified distance.
        /// </summary>
        /// <remarks>
        /// Please note that this function doesn't have any interpolation, it works as teleport.
        /// </remarks>
        /// <param name="distance">Distance in meters.</param>
        public void Move(float distance)
        {
            distance *= Game.FPS;

            Speed += distance;
            moveStack.Add((distance, Game.FrameCount));
        }
    }
}
