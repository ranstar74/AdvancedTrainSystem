using AdvancedTrainSystem.Railroad.Components;
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
            //if (Game.IsControlJustPressed(Control.ThrowGrenade))
            //{
            //    derail.Derail();
            //    ((SteamTrainComponentCollection)train.Components).Controls.Throttle = 1;
            //    ((SteamTrainComponentCollection)train.Components).Controls.Gear = 1;
            //}

            UpdateWheelSpeed();
            
            // Since hidden vehicle isn't used after derail we just set speed on zero
            if (derail.IsDerailed)
            {
                Speed = 0;
                return;
            }

            float acceleration = (Speed - _prevSpeed) * Game.LastFrameTime;

            // Train don't really go uphill
            float slopeForce = train.Rotation.X / 4;
            
            // These are not fully physical based but i found these values
            // to work good enough
            float dragForce = (float) (0.02f * Math.Pow(Speed, 2)) / 8;
            float inerciaForce = acceleration * 5;
            float frictionForce = 0.2f * Speed / 2;

            // Make train don't accelerate if wheel slips
            float slipForce = WheelSlip * _newForces * 200;

            float totalForce = slopeForce + dragForce + inerciaForce + frictionForce + slipForce;

            ApplyForce(-totalForce * _forceMultiplier * Game.LastFrameTime);

            _prevSpeed = Speed;

            // Apply forces of this frame to speed
            Speed += _newForces;
            _newForces = 0f;
        }

        /// <summary>
        /// Calculates drive wheel speed.
        /// It is different from actual speed if wheel slipping or locked by brake.
        /// </summary>
        private void UpdateWheelSpeed()
        {
            // Im not really sure what is the best way to get slip
            // speed, so we're faking it with new forces applied to train (which is mostly engine force)
            float slipSpeed = _newForces * 45 / Game.LastFrameTime;
            float wheelSpeedTo = DoWheelSlip ? slipSpeed : VisualSpeed;

            // Can't really think of a way calculating these in one line
            // And since wheel slip is faked im not sure there point to
            WheelSlip = MathExtensions.Lerp(WheelSlip, DoWheelSlip ? 1f : 0f, Game.LastFrameTime * 2);
            DriveWheelSpeed = MathExtensions.Lerp(DriveWheelSpeed, wheelSpeedTo, Game.LastFrameTime * 2);
        }

        /// <summary>
        /// Applies some external force on train.
        /// </summary>
        public void ApplyForce(float force)
        {
            // If train is not derailed, apply new forces to train speed,
            // otherwise, we apply forces to entity, making it move
            // Not sure it works like that irl but that looks fun
            if(!derail.IsDerailed)
            {
                _newForces += force;
            }
            else
            {
                Vehicle vehicle = train;

                // Not sure if this bool works right on trains...
                if(vehicle.IsOnAllWheels)
                    vehicle.ApplyForce(
                        direction: train.ForwardVector * force * 35, 
                        rotation: Vector3.Zero, 
                        forceType: ForceType.MaxForceRot);
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
