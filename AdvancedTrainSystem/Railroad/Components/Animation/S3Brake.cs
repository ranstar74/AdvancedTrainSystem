using AdvancedTrainSystem.Core.Components;
using AdvancedTrainSystem.Railroad.Components.Steam;
using FusionLibrary;
using GTA;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.AnimComponents
{
    public class S3Brake : TrainComponent
    {
        private AnimateProp AirbrakeMain;
        private AnimateProp AirbrakeRod;
        private AnimateProp AirbrakeLever;

        private readonly AnimatePropsHandler Brakes = new AnimatePropsHandler();

        private const float _airbrakeMainOffset = 0.1f;
        private const float _airbrakeLeverOffset = -6;
        private const float _brakeAngle = -5;

        private Airbrake airbrake;
        private SteamControls _controls;

        public S3Brake(ComponentCollection components) : base(components)
        {
            
        }

        public override void Start()
        {
            base.Start();

            _controls = Components.GetComponent<SteamControls>();
            airbrake = Components.GetComponent<Airbrake>();

            AirbrakeMain = new AnimateProp(ModelHandler.SierraAirbrakeMain, Train, "chassis");
            AirbrakeRod = new AnimateProp(ModelHandler.SierraAirbrakeRod, Train, "chassis");
            AirbrakeLever = new AnimateProp(ModelHandler.SierraAirbrakeLever, Train, "airbrake_lever");

            Brakes.Add(new AnimateProp(ModelHandler.SierraBrake1, Train, "brake_1"));
            Brakes.Add(new AnimateProp(ModelHandler.SierraBrake2, Train, "brake_2"));
            Brakes.Add(new AnimateProp(ModelHandler.SierraBrake3, Train, "brake_3"));

            Brakes.SpawnProp();
            AirbrakeMain.SpawnProp();
            AirbrakeRod.SpawnProp();
            AirbrakeLever.SpawnProp();
        }

        public override void Update()
        {
            var airbrakeForce = airbrake.Force;

            var mainOffset = _airbrakeMainOffset * airbrakeForce;
            var rodOffset = ((Vehicle)Train).GetPositionOffset(
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
