using AdvancedTrainSystem.Railroad.Components;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// For some reason TrainSetSpeed function cut any speed below about 0.1~,
        /// we don't want wheels to spin when train is still so this 
        /// speed needs to be used for anything graphical such as wheels.
        /// <para>
        /// <see cref="DriveWheelSpeed"/> uses <see cref="VisualSpeed "/> internally.
        /// </para>
        /// </summary>
        public float VisualSpeed => AbsoluteSpeed > 0.125f ? speed : 0;

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

        /// <summary>
        /// Average speed within last 1 second.
        /// </summary>
        public float AverageSpeed { get; internal set; }

        private readonly Dictionary<int, float> _speeds = new Dictionary<int, float>();
        private const float _forceMultiplier = 0.2f;
        private float _prevSpeed;
        private float _newForces = 0f;
        private int _averageUpdateTime = 0;

        private readonly Train train;
        private DerailComponent _derail;
        private CollisionComponent collision;

        public PhysxComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            _derail = Components.GetComponent<DerailComponent>();
            collision = Components.GetComponent<CollisionComponent>();
        }

        public override void Update()
        {
            UpdateWheelSpeed();
            UpdateAverageSpeed();

            // Since hidden vehicle isn't used after derail we just set speed on zero
            if (_derail.IsDerailed)
            {
                //Speed = 0;
                return;
            }

            float acceleration = (Speed - _prevSpeed) * Game.LastFrameTime;

            // Train don't really go uphill
            float slopeForce = train.Rotation.X / 4;
            
            // These are not fully physical based but i found these values
            // to work good enough
            float dragForce = (float) (0.02f * Math.Pow(Speed, 2)) / 8;
            float inerciaForce = -_newForces / 10;
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
        /// Calculates averega speed within last second.
        /// </summary>
        private void UpdateAverageSpeed()
        {
            // May happen if game is in slow motion
            if(!_speeds.Keys.Contains(Game.GameTime))
                _speeds.Add(Game.GameTime, Speed);

            List<int> gameTimeToRemove = _speeds
                .Where(x => Game.GameTime - x.Key > 1000)
                .Select(x => x.Key)
                .ToList();

            foreach(int time in gameTimeToRemove)
            {
                _speeds.Remove(time);
            }

            if(_averageUpdateTime < Game.GameTime)
            {
                AverageSpeed = _speeds.Sum(x => x.Value) / _speeds.Count;
                _averageUpdateTime = Game.GameTime + 1000;
            }
        }

        /// <summary>
        /// Calculates drive wheel speed.
        /// It is different from actual speed if wheel slipping or locked by brake.
        /// </summary>
        private void UpdateWheelSpeed()
        {
            // Im not really sure what is the best way to get slip
            // speed, so we're faking it with new forces applied to train (which is mostly engine force)

            // No idea why but when train derailed wheel slip speed gets to some fucking high values
            // so here's hack to lower it
            float slipMultiplier = _derail.IsDerailed ? 2 : 60;
            float slipSpeed = _newForces * slipMultiplier / Game.LastFrameTime;

            // Im too tired of this fucking high speed, i have no fucking clue why it doesnt work
            // and debugging it is fucking hell SO EAT UR ASS U DUMB FUCK IM CLIPPING U
            if(_derail.IsDerailed)
            {
                slipSpeed = MathExtensions.Clamp(slipSpeed, -25, 25);
            }

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
            _newForces += force;

            // We apply forces to entity, making it move
            // Not sure it works like that irl but that looks fun
            if (_derail.IsDerailed)
            {
                Vehicle vehicle = train;

                RaycastResult ray = World.Raycast(
                    source: train.Position,
                    direction: Vector3.WorldDown,
                    maxDistance: 0.5f,
                    options: IntersectFlags.Map);

                if(ray.DidHit)
                    vehicle.ApplyForce(
                        direction: train.ForwardVector * force * 40, 
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
