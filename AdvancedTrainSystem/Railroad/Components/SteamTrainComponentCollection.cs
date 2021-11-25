using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Railroad.Components.AnimComponents;
using AdvancedTrainSystem.Railroad.Components.SteamComponents;
using AdvancedTrainSystem.Railroad.SharedComponents;

namespace AdvancedTrainSystem.Railroad.Components
{
    /// <summary>
    /// This class contains all <see cref="SteamTrain"/> components.
    /// </summary>
    public class SteamTrainComponentCollection : TrainComponentCollection
    {
        public BoilerComponent Boiler;
        public DynamoComponent Generator;
        public LightComponent Light;
        public AirbrakeComponent Airbrake;
        public ControlsComponent Controls;
        public SteamEngineComponent SteamEngine;
        public SteamSoundsComponent SteamSounds;
        public SafetyValveComponent SafetyValve;
        public SteamParticleComponent SteamParticle;
        public ChimneyComponent Chimney;
        public SteamGaugesComponent SteamGauges;

        // To remove later...
        public SierraDrivetrainAnimComponent SierraDrivetrainAnimComponent;
        public SierraWheelAnimComponent SierraWheelAnimComponent;
        public SierraBrakeAnimComponent SierraBrakeAnimComponent;

        /// <summary>
        /// Creates a new instance of <see cref="SteamTrainComponentCollection"/>.
        /// </summary>
        /// <param name="train"></param>
        public SteamTrainComponentCollection(SteamTrain train) : base(train)
        {
            Boiler = Create<BoilerComponent>();
            Generator = Create<DynamoComponent>();
            Light = Create<LightComponent>();
            Airbrake = Create<AirbrakeComponent>();
            Controls = Create<ControlsComponent>();
            SteamEngine = Create<SteamEngineComponent>();
            SteamSounds = Create<SteamSoundsComponent>();
            SafetyValve = Create<SafetyValveComponent>();
            SteamParticle = Create<SteamParticleComponent>();
            Chimney = Create<ChimneyComponent>();
            SteamGauges = Create<SteamGaugesComponent>();

            SierraDrivetrainAnimComponent = Create<SierraDrivetrainAnimComponent>();
            SierraWheelAnimComponent = Create<SierraWheelAnimComponent>();
            SierraBrakeAnimComponent = Create<SierraBrakeAnimComponent>();

            OnStart();
        }
    }
}
