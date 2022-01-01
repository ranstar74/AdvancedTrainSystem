using AdvancedTrainSystem.Core.Components;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// This class contains all the <see cref="Train"/> components.
    /// </summary>
    public abstract class TrainComponentCollection : ComponentCollection
    {
        /// <inheritdoc cref="Components.Physx"/>
        public Physx Physx { get; private set; }

        /// <inheritdoc cref="Components.Sounds"/>
        public Sounds Sounds { get; private set; }

        /// <inheritdoc cref="Components.Particles"/>
        public Particles Particle { get; private set; }

        /// <inheritdoc cref="Components.Driving"/>
        public Driving Driving { get; private set; }

        /// <inheritdoc cref="Components.Derail"/>
        public Derail Derail { get; private set; }

        /// <inheritdoc cref="Components.Collision"/>
        public Collision Collision { get; private set; }

        /// <inheritdoc cref="Components.CinematicCamera"/>
        public CinematicCamera Camera { get; private set; }

        /// <inheritdoc cref="Components.Motion"/>
        public Motion Motion { get; private set; }

        /// <summary>Creates a new <see cref="TrainComponentCollection"/> instance.</summary>
        public TrainComponentCollection(Train train) : base(train)
        {
            Physx = Create<Physx>();
            Sounds = Create<Sounds>();
            Particle = Create<Particles>();
            Driving = Create<Driving>();
            Derail = Create<Derail>();
            Collision = Create<Collision>();
            Camera = Create<CinematicCamera>();
            Motion = Create<Motion>();

            OnStart();
        }
    }
}
