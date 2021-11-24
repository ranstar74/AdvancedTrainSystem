using AdvancedTrainSystem.Core.Utils;
using AdvancedTrainSystem.Railroad.Components.SteamComponents;
using FusionLibrary;
using GTA.Math;
using RageComponent;
using RageComponent.Core;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Core.Components.Abstract
{
    public class ParticleComponent : Component
    {
        protected readonly ParticlePlayerHandler _wheelSparks = new ParticlePlayerHandler();
        protected readonly Train train;

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
            _wheelSparks.SetEvolutionParam("LOD", 1);
            _wheelSparks.SetEvolutionParam("squeal", 1);
        }

        public override void Update()
        {
            _wheelSparks.SetState(_physx.DoWheelSlip);

            // Spark will be flipped if train is either braking in reverse or wheel slip in reverse
            bool sparksFlipped = _physx.Speed < 0 || _physx.DriveWheelSpeed < 0;
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
