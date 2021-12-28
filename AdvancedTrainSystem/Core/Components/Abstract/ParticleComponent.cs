using AdvancedTrainSystem.Core.Utils;
using AdvancedTrainSystem.Railroad.Components.SteamComponents;
using FusionLibrary;
using GTA.Math;
using GTA.UI;
using RageComponent;
using RageComponent.Core;
using System;
using System.Linq;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Core.Components.Abstract
{
    public class ParticleComponent : Component
    {
        protected readonly ParticlePlayerHandler _wheelSparks = new ParticlePlayerHandler();
        protected readonly ParticlePlayerHandler _wheelSparksLeft = new ParticlePlayerHandler();
        protected readonly ParticlePlayerHandler _wheelSparksRight = new ParticlePlayerHandler();

        protected readonly Train train;

        protected DerailComponent _derail;
        protected PhysxComponent _physx;
        protected ControlsComponent _controls;

        public ParticleComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            _physx = Components.GetComponent<PhysxComponent>();
            _controls = Components.GetComponent<ControlsComponent>();
            _derail = Components.GetComponent<DerailComponent>();

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
                    entity: train, 
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
            _wheelSparks.SetState(_physx.DoWheelSlip && _physx.DriveWheelSpeed > 5f);

            if(Math.Abs(_derail.Angle) > 0.4f && !_derail.IsDerailed)
            {
                bool rightSparks = _derail.Angle >= 0;

                _wheelSparksRight.SetState(rightSparks);
                _wheelSparksLeft.SetState(!rightSparks);
            }

            // Spark will be flipped if train is either braking in reverse or wheel slip in reverse
            // TODO: When steam brake will be added, spark direction would be wrong
            bool sparksFlipped = _physx.DriveWheelSpeed < 0;
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
