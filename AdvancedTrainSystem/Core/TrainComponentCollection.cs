using AdvancedTrainSystem.Core.Components;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// This class contains all the <see cref="Train"/> components.
    /// </summary>
    public abstract class TrainComponentCollection : ComponentCollection
    {
        /// <summary>
        /// Handles train speed.
        /// </summary>
        public PhysxComponent PhysxComponent;

        /// <summary>
        /// Handles train collision.
        /// </summary>
        public CollisionComponent CollisionComponent;

        /// <summary>
        /// Handles train derailnment.
        /// </summary>
        public DerailComponent DerailComponent;

        /// <summary>
        /// Defines behaviour of train cab camera.
        /// </summary>
        public CameraComponent CameraComponent;

        /// <summary>
        /// Defines base enter / leave train actions.
        /// </summary>
        public DrivingComponent DrivingComponent;

        /// <summary>
        /// Creates a new instance of <see cref="TrainComponentCollection"/>.
        /// </summary>
        public TrainComponentCollection(Train train) : base(train)
        {
            PhysxComponent = Create<PhysxComponent>();
            CollisionComponent = Create<CollisionComponent>();
            DerailComponent = Create<DerailComponent>();
            CameraComponent = Create<CameraComponent>();
            DrivingComponent = Create<DrivingComponent>();

            OnStart();
        }
    }
}
