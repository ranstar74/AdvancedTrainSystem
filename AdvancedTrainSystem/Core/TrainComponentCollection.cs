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
        public PhysxComponent Physx;

        /// <summary>
        /// Handles train collision.
        /// </summary>
        public CollisionComponent Collision;

        /// <summary>
        /// Handles train derailnment.
        /// </summary>
        public DerailComponent Derail;

        /// <summary>
        /// Defines behaviour of train cab camera.
        /// </summary>
        public CameraComponent Camera;

        /// <summary>
        /// Defines base enter / leave train actions.
        /// </summary>
        public DrivingComponent Driving;

        /// <summary>
        /// Creates a new instance of <see cref="TrainComponentCollection"/>.
        /// </summary>
        public TrainComponentCollection(Train train) : base(train)
        {
            Physx = Create<PhysxComponent>();
            Collision = Create<CollisionComponent>();
            Derail = Create<DerailComponent>();
            Camera = Create<CameraComponent>();
            Driving = Create<DrivingComponent>();

            OnStart();
        }
    }
}
