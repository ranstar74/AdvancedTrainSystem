using AdvancedTrainSystem.Core.Utils;
using FusionLibrary;
using GTA.Math;
using RageComponent.Core;
using System;
using System.Linq;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Handles train particle effects.</summary>
    public class Particles : TrainComponent
    {
        private readonly ParticlePlayerHandler _wheelSparks = new ParticlePlayerHandler();
        private readonly ParticlePlayerHandler _wheelSparksLeft = new ParticlePlayerHandler();
        private readonly ParticlePlayerHandler _wheelSparksRight = new ParticlePlayerHandler();

        public Particles(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            // Spark particles
            BoneUtils.ProcessSideBones(
                baseBone: "dhweel_spark_", 
                totalBoneNumber: 6, 
                action: bone =>
            {
                _wheelSparks.Add(
                    assetName:"core", 
                    effectName: "veh_train_sparks", 
                    particleType: ParticleType.Looped, 
                    entity: Train, 
                    boneName: bone);
            });

            var lefts = _wheelSparks.ParticlePlayers.Where(x => x.BoneName.Contains("left"));
            var rights = _wheelSparks.ParticlePlayers.Where(x => x.BoneName.Contains("right"));
            _wheelSparksLeft.ParticlePlayers.AddRange(lefts);
            _wheelSparksRight.ParticlePlayers.AddRange(rights);

            _wheelSparks.SetEvolutionParam("LOD", 1);
            _wheelSparks.SetEvolutionParam("squeal", 1);
        }

        public override void Update()
        {
            // TODO: || _physx.WheelLocked
            _wheelSparks.SetState((Physx.DoWheelSlip && Physx.DriveWheelSpeed > 5f) || (Physx.AreDriveWheelsLockedThisFrame && Physx.AbsoluteSpeed > 2f));

            if(Math.Abs(Motion.Angle) > 0.4f && !Derail.IsDerailed && !Physx.AreDriveWheelsLockedThisFrame)
            {
                bool rightSparks = Motion.Angle >= 0;

                _wheelSparksRight.SetState(rightSparks);
                _wheelSparksLeft.SetState(!rightSparks);
            }

            // Spark will be flipped if train is either braking in reverse or wheel slip in reverse
            // TODO: When steam brake will be added, spark direction would be wrong
            bool sparksFlipped = Physx.DriveWheelSpeed < 0;
            Vector3 sparkRotation = sparksFlipped ? new Vector3(190, 0, 0) : Vector3.Zero;

            for (int i = 0; i < _wheelSparks.ParticlePlayers.Count; i++)
                _wheelSparks[i].Rotation = sparkRotation;
        }

        public override void Dispose()
        {
            _wheelSparks.Dispose();
        }
    }
}
