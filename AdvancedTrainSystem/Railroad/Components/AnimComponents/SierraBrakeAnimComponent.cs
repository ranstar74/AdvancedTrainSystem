using AdvancedTrainSystem.Core.Components.Abstract.AnimComponents;
using AdvancedTrainSystem.Railroad.Components.SteamComponents;
using FusionLibrary;
using GTA;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedTrainSystem.Railroad.Components.AnimComponents
{
    public class SierraBrakeAnimComponent : AnimComponent
    {
        private AnimateProp AirbrakeMain;
        private AnimateProp AirbrakeRod;
        private AnimateProp AirbrakeLever;

        private readonly AnimatePropsHandler Brakes = new AnimatePropsHandler();

        private const float _airbrakeMainOffset = 0.1f;
        private const float _airbrakeLeverOffset = -6;
        private const float _brakeAngle = -5;

        private AirbrakeComponent airbrake;

        public SierraBrakeAnimComponent(ComponentCollection components) : base(components)
        {
            
        }

        public override void Start()
        {
            base.Start();

            airbrake = Components.GetComponent<AirbrakeComponent>();

            AirbrakeMain = new AnimateProp(ModelHandler.SierraAirbrakeMain, train, "chassis");
            AirbrakeRod = new AnimateProp(ModelHandler.SierraAirbrakeRod, train, "chassis");
            AirbrakeLever = new AnimateProp(ModelHandler.SierraAirbrakeLever, train, "airbrake_lever");

            Brakes.Add(new AnimateProp(ModelHandler.SierraBrake1, train, "brake_1"));
            Brakes.Add(new AnimateProp(ModelHandler.SierraBrake2, train, "brake_2"));
            Brakes.Add(new AnimateProp(ModelHandler.SierraBrake3, train, "brake_3"));

            Brakes.SpawnProp();
            AirbrakeMain.SpawnProp();
            AirbrakeRod.SpawnProp();
            AirbrakeLever.SpawnProp();
        }

        public override void Update()
        {
            var airbrakeForce = airbrake.Intensity;

            var mainOffset = _airbrakeMainOffset * airbrakeForce;
            var rodOffset = ((Vehicle)train).GetPositionOffset(
                AirbrakeLever.Prop.Bones["airbrake_rod_mount"].Position);
            var leverAngle = _airbrakeLeverOffset * airbrakeForce;
            var brakeAngle = _brakeAngle * airbrakeForce;

            AirbrakeMain.SetOffset(FusionEnums.Coordinate.Y, mainOffset);
            AirbrakeRod.SetOffset(rodOffset);
            AirbrakeLever.SetRotation(FusionEnums.Coordinate.X, leverAngle);
            Brakes.SetRotation(FusionEnums.Coordinate.X, brakeAngle);
        }

        public override void Dispose()
        {
            Brakes.Dispose();
            AirbrakeMain.Dispose();
            AirbrakeRod.Dispose();
            AirbrakeLever.Dispose();
        }
    }
}
