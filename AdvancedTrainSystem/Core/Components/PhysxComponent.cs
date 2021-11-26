using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent;
using RageComponent.Core;
using System;

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
        /// For some reason TrainSetSpeed function cut any speed below about 0.15,
        /// we don't want wheels to spin when train is still so this 
        /// speed needs to be used for anything graphical such as wheels.
        /// <para>
        /// <see cref="DriveWheelSpeed"/> uses <see cref="VisualSpeed "/> internally.
        /// </para>
        /// </summary>
        public float VisualSpeed => AbsoluteSpeed > 0.15f ? speed : 0;

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
        public float AbsoluteSpeed => Math.Abs(Speed);

        /// <summary>
        /// Drive wheels speed. Could be higher than train speed if train slips.
        /// </summary>
        public float DriveWheelSpeed { get; private set; }

        /// <summary>
        /// Gets a normalized value indicating how much wheels slip.
        /// </summary>
        public float WheelSlip { get; private set; }

        /// <summary>
        /// Whether wheel slip or not.
        /// </summary>
        /// <remarks>
        /// Controlled by engine component, if theres any.
        /// </remarks>
        public bool DoWheelSlip { get; internal set; }

        private const float _forceMultiplier = 0.2f;
        private float _prevSpeed;
        private float _newForces = 0f;

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
            UpdateWheelSpeed();
            
            // Since hidden vehicle isn't used after derail we just set speed on zero
            if (derail.IsDerailed)
            {
                Speed = 0;
                return;
            }

            // TODO: Take uphill/downhill into account

            float acceleration = (Speed - _prevSpeed) * Game.LastFrameTime;

            float dragForce = (float) (0.02f * Math.Pow(Speed, 2)) / 8;
            float inerciaForce = acceleration * 5;
            float frictionForce = 0.2f * Speed / 2;
            float slipForce = WheelSlip * _newForces * 200;

            float totalForce = dragForce + inerciaForce + frictionForce + slipForce;

            ApplyForce(-totalForce * _forceMultiplier * Game.LastFrameTime);

            _prevSpeed = Speed;

            // Apply forces of this frame to speed
            Speed += _newForces;
            _newForces = 0f;
        }

        private void UpdateWheelSpeed()
        {
            float slipSpeed = _newForces >= 0 ? 22f : -22f;
            float wheelSpeedTo = DoWheelSlip ? slipSpeed : VisualSpeed;

            // Can't really think of a way calculating these in one
            // And since wheel slip is faked im not sure there point to
            WheelSlip = MathExtensions.Lerp(WheelSlip, DoWheelSlip ? 1f : 0f, Game.LastFrameTime * 2);
            DriveWheelSpeed = MathExtensions.Lerp(DriveWheelSpeed, wheelSpeedTo, Game.LastFrameTime * 2);
        }

        /// <summary>
        /// Applies some external force on train.
        /// </summary>
        public void ApplyForce(float force)
        {
            if(!derail.IsDerailed)
            {
                _newForces += force;
            }
            else
            {
                // TODO: Fix train doesn't have physics...
                ((Vehicle)train).ApplyForce(train.ForwardVector * force * 100, Vector3.Zero, ForceType.MinForce2);
            }
        }

        /// <summary>
        /// Applies some external force on train on track speed.
        /// </summary>
        public void ApplyTrackForce(float force)
        {
            TrackSpeed += force;
        }
    }
}
