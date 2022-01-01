using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Controls train motion. Angle on turns, shaking while driving.</summary>
    public class Motion : TrainComponent
    {
        /// <summary>Gets train locomotive angle on Y axis.</summary>
        public float Angle => _carriagePrevVecs[0].Rotation.Y;

        /// <summary>Gets a vector noise value that is used to simulate train shaking on speed.</summary>
        public Vector3 Noise { get; private set; } = default;

        private readonly List<RotationInfo> _carriagePrevVecs = new List<RotationInfo>();

        /// <summary>Contains information for calculating train angle on turns.</summary>
        private class RotationInfo
        {
            public TrainCarriage Carriage { get; set; }
            public Vector3 PrevForwardVector { get; set; }
            public Vector3 Rotation { get; set; }

            public RotationInfo(TrainCarriage carriage)
            {
                Carriage = carriage;
                PrevForwardVector = carriage.Vehicle.ForwardVector;
            }
        }

        public Motion(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            Train.Carriages.ForEach(carriage =>
            {
                _carriagePrevVecs.Add(new RotationInfo(carriage));
            });
        }

        public override void Update()
        {
            if (Derail.IsDerailed)
                return;
            
            // Create noise that adds some life to train when it moves...
            Vector3 newNoise = new Vector3(
                (float)FusionUtils.Random.NextDouble(),
                (float)FusionUtils.Random.NextDouble(),
                (float)FusionUtils.Random.NextDouble());
            newNoise *= 0.5f;

            // Same as above on frameAngle, higher speed = more noise
            float noiseAmplitude = Physx.AbsoluteSpeed / 15;
            noiseAmplitude = noiseAmplitude.Clamp(0f, 1.25f);

            newNoise *= noiseAmplitude;

            // Make noise more "shaky" when speed raises
            float noiseSpeed = Physx.AbsoluteSpeed / 5;
            noiseSpeed = noiseSpeed.Clamp(0f, 3f);

            Noise = Vector3.Lerp(Noise, newNoise, Game.LastFrameTime * noiseSpeed);

            // Explained below
            float speedFactor = Game.LastFrameTime * 10000 * Physx.AbsoluteSpeed / 200;
            for (int i = 0; i < _carriagePrevVecs.Count; i++)
            {
                RotationInfo rotInfo = _carriagePrevVecs[i];
                TrainCarriage carriage = rotInfo.Carriage;

                Vector3 forwardVector = carriage.Vehicle.ForwardVector;

                // Find angle by difference of forward vectors of this and previous frame
                float frameAngle = Vector3.SignedAngle(forwardVector, rotInfo.PrevForwardVector, Vector3.WorldUp);

                // Since frameAngle is too low, we first multiply it on 10000 (just value i found work good)
                // Then we multiply it one (Speed / 200), so on 200 m/s we will get multiplier 
                //      Which basically will give higher angle on higher speed,
                //      lower value to get higher angle on lower speeds
                frameAngle *= speedFactor;

                // Make angle non linear
                frameAngle *= Math.Abs(frameAngle);
                frameAngle /= 10;

                // Not sure how this happens but it does
                if (float.IsNaN(frameAngle))
                    frameAngle = 0f;

                ApplyAngleOnCarriage(carriage, frameAngle, rotInfo);

                _carriagePrevVecs[i].PrevForwardVector = forwardVector;
            }
        }

        private void ApplyAngleOnCarriage(TrainCarriage carriage, float angle, RotationInfo rotInfo)
        {
            // For some reason game crashes on extreme angles,
            // we derail on 30 anyway to its irrelevant
            angle = Math.Min(angle, 60f);

            Vector3 rotation = new Vector3(0, angle, 0);
            rotInfo.Rotation = Vector3.Lerp(rotInfo.Rotation, rotation, Game.LastFrameTime);

            carriage.Vehicle.AttachTo(
                entity: carriage.HiddenVehicle,
                rotation: rotInfo.Rotation + Noise);
        }
    }
}
