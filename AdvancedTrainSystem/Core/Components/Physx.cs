﻿using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Calculates speed of the train.</summary>
    public class Physx : TrainComponent
    {
        private float speed;
        /// <summary>Speed of the train.</summary>
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
                if (Collision.IsTrainCoupled && (Game.FrameCount - Collision.CoupleFrame) > 5)
                {
                    Train.TrainLocomotive.HiddenVehicle.SetTrainCruiseSpeed(speed);
                }
                else
                {
                    Train.TrainLocomotive.HiddenVehicle.SetTrainCruiseSpeed(0);
                    Train.TrainLocomotive.HiddenVehicle.SetTrainSpeed(speed);
                }
            }
        }

        /// <summary>For some reason TrainSetSpeed function cut any speed below about 0.1~,
        /// we don't want wheels to spin when train is still so this 
        /// speed needs to be used for anything graphical such as wheels.
        /// <para><see cref="DriveWheelSpeed"/> uses <see cref="VisualSpeed "/> internally.</para>
        /// </summary>
        public float VisualSpeed => AbsoluteSpeed > 0.135f ? speed : 0;

        /// <summary>Track speed is not depends on train direction. 
        /// Can be used if u want to move two trains with different direction with same direction.</summary>
        public float TrackSpeed
        {
            get => Train.Direction ? Speed : -Speed;
            set
            {
                if (Train.Direction)
                    Speed = value;
                else
                    Speed = -value;
            }
        }

        /// <summary>Absolute speed of the train.</summary>
        public float AbsoluteSpeed => Math.Abs(Speed);

        /// <summary>Drive wheels speed. Could be higher than train speed if train slips.</summary>
        public float DriveWheelSpeed { get; private set; }

        /// <summary>Gets a normalized value indicating how much wheels slip.</summary>
        public float WheelSlip { get; private set; }

        /// <summary>Whether wheel slip or not.</summary>
        /// <remarks>Controlled by engine component, if theres any.</remarks>
        public bool DoWheelSlip { get; internal set; }

        /// <summary>Average speed within last 1 second.</summary>
        public float AverageSpeed { get; internal set; }

        /// <summary>Train acceleration this frame.</summary>
        public float TrainAcceleration { get; private set; }

        /// <summary>Gets or sets a value indicating whether drive wheels are locked or not.</summary>
        public bool AreDriveWheelsLockedThisFrame { get; set; }

        /// <summary>Gets or sets a value that defines whether drive forces needs to be applied
        /// this frame or not.</summary>
        public bool DontApplyDriveForcesThisFrame { get; set; }

        private const float _forceMultiplier = 0.2f;
        private readonly Dictionary<int, float> _speeds = new Dictionary<int, float>();
        private float _prevSpeed;
        private float _newDriveForces = 0f;
        private float _newResistanceForces = 0f;
        private int _averageUpdateTime = 0;

        public Physx(ComponentCollection components) : base(components)
        {

        }

        public override void Update()
        {
            UpdateWheelSpeed();
            UpdateAverageSpeed();

            // Since hidden vehicle isn't used after derail we just set speed on zero
            if (Derail.IsDerailed)
            {
                //Speed = 0;
                return;
            }
            TrainAcceleration = (Speed - _prevSpeed) * Game.LastFrameTime;

            // Train don't really go uphill
            float slopeForce = Train.Rotation.X / 4;

            // These are not fully physical based but i found these values
            // to work good enough
            float dragForce = (float)(0.02f * Math.Pow(Speed, 2)) / 8;
            float inerciaForce = -_newDriveForces / 10;
            float frictionForce = 0.2f * Speed / 2;

            // Make train don't accelerate if wheel slips
            float slipForce = WheelSlip * _newDriveForces * 200;

            float totalForce = slopeForce + dragForce + inerciaForce + frictionForce + slipForce;

            ApplyResistanceForce(-totalForce * _forceMultiplier * Game.LastFrameTime);

            _prevSpeed = Speed;

            if (DontApplyDriveForcesThisFrame)
            {
                _newDriveForces = 0f;
            }

            // Apply forces of this frame to speed
            Speed += _newDriveForces + _newResistanceForces;

            _newDriveForces = 0f;
            _newResistanceForces = 0f;
        }

        public override void LateUpdate()
        {
            DontApplyDriveForcesThisFrame = false;
            AreDriveWheelsLockedThisFrame = false;
        }

        /// <summary>Calculates averega speed within last second.</summary>
        private void UpdateAverageSpeed()
        {
            // May happen if game is in slow motion
            if (!_speeds.Keys.Contains(Game.GameTime))
                _speeds.Add(Game.GameTime, Speed);

            List<int> gameTimeToRemove = _speeds
                .Where(x => Game.GameTime - x.Key > 1000)
                .Select(x => x.Key)
                .ToList();

            foreach (int time in gameTimeToRemove)
            {
                _speeds.Remove(time);
            }

            if (_averageUpdateTime < Game.GameTime)
            {
                AverageSpeed = _speeds.Sum(x => x.Value) / _speeds.Count;
                _averageUpdateTime = Game.GameTime + 1000;
            }
        }

        /// <summary>Calculates drive wheel speed.
        /// It is different from actual speed if wheel slipping or locked by brake.</summary>
        private void UpdateWheelSpeed()
        {
            // Im not really sure what is the best way to get slip
            // speed, so we're faking it with new forces applied to train (which is mostly engine force)

            // No idea why but when train derailed wheel slip speed gets to some fucking high values
            // so here's hack to lower it
            float slipMultiplier = Derail.IsDerailed ? 2 : 60;
            float slipSpeed = _newDriveForces * slipMultiplier / Game.LastFrameTime;

            // Im too tired of this fucking high speed, i have no fucking clue why it doesnt work
            // and debugging it is fucking hell SO EAT UR ASS U DUMB FUCK IM CLIPPING U
            float sLimit = 35;
            slipSpeed = MathExtensions.Clamp(slipSpeed, -sLimit, sLimit);

            float wheelSpeedTo = DoWheelSlip ? slipSpeed : VisualSpeed.Clamp(-sLimit, sLimit);

            if (AreDriveWheelsLockedThisFrame)
            {
                wheelSpeedTo = 0f;
            }

            // Can't really think of a way calculating these in one line
            // And since wheel slip is faked im not sure there point to
            WheelSlip = MathExtensions.Lerp(WheelSlip, DoWheelSlip ? 1f : 0f, Game.LastFrameTime * 2);
            DriveWheelSpeed = MathExtensions.Lerp(DriveWheelSpeed, wheelSpeedTo, Game.LastFrameTime * 2);
        }

        /// <summary>Applies drive force on this train.</summary>
        public void ApplyDriveForce(float force)
        {
            _newDriveForces += force;

            // We apply forces to entity, making it move
            // Not sure it works like that irl but that looks fun
            if (Derail.IsDerailed)
            {
                Vehicle vehicle = Train;

                RaycastResult ray = World.Raycast(
                    source: Train.Position,
                    direction: Vector3.WorldDown,
                    maxDistance: 0.5f,
                    options: IntersectFlags.Map);

                if (ray.DidHit)
                    vehicle.ApplyForce(
                        direction: Train.ForwardVector * force * 40,
                        rotation: Vector3.Zero,
                        forceType: ForceType.MaxForceRot);
            }
        }

        /// <summary>Applies resistance force on this train.</summary>
        public void ApplyResistanceForce(float force)
        {
            _newResistanceForces += force;
        }

        /// <summary>Applies some external force on train on track speed.</summary>
        public void ApplyTrackForce(float force)
        {
            TrackSpeed += force;
        }
    }
}
