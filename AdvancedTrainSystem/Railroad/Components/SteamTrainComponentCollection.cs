using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Railroad.Components.AnimComponents;
using AdvancedTrainSystem.Railroad.Components.Common;
using AdvancedTrainSystem.Railroad.Components.Steam;

namespace AdvancedTrainSystem.Railroad.Components
{
    /// <summary>
    /// This class contains all <see cref="SteamTrain"/> components.
    /// </summary>
    public class SteamTrainComponentCollection : TrainComponentCollection
    {
        public Boiler Boiler;
        public Dynamo Dynamo;
        public Illumination Illumination;
        public Airbrake Airbrake;
        public SteamControls SteamControls;
        public SteamEngine SteamEngine;
        public SteamSounds SteamSounds;
        public SafetyValve SafetyValve;
        public SteamParticles SteamParticles;
        public Chimney Chimney;
        public SteamGauges SteamGauges;
        public Hydrobrake Hydrobrake;

        // To remove later...
        public S3Drivetrain S3Drivetrain;
        public S3Wheel S3Wheel;
        public S3Brake S3Brake;

        /// <summary>
        /// Creates a new instance of <see cref="SteamTrainComponentCollection"/>.
        /// </summary>
        /// <param name="train"></param>
        public SteamTrainComponentCollection(SteamTrain train) : base(train)
        {
            Boiler = Create<Boiler>();
            Dynamo = Create<Dynamo>();
            Illumination = Create<Illumination>();
            Airbrake = Create<Airbrake>();
            SteamControls = Create<SteamControls>();
            SteamEngine = Create<SteamEngine>();
            SteamSounds = Create<SteamSounds>();
            SafetyValve = Create<SafetyValve>();
            SteamParticles = Create<SteamParticles>();
            Chimney = Create<Chimney>();
            SteamGauges = Create<SteamGauges>();
            Hydrobrake = Create<Hydrobrake>();

            S3Drivetrain = Create<S3Drivetrain>();
            S3Wheel = Create<S3Wheel>();
            S3Brake = Create<S3Brake>();

            OnStart();
        }
    }
}
