using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Contains all core train components.</summary>
    public abstract class TrainComponent : Component
    {        
        /// <inheritdoc cref="Components.Physx"/>
        protected Physx Physx { get; private set; }

        /// <inheritdoc cref="Components.Sounds"/>
        protected Sounds Sounds { get; private set; }

        /// <inheritdoc cref="Components.Particles"/>
        protected Particles Particles { get; private set; }

        /// <inheritdoc cref="Components.Driving"/>
        protected Driving Driving { get; private set; }

        /// <inheritdoc cref="Components.Derail"/>
        protected Derail Derail { get; private set; }

        /// <inheritdoc cref="Components.Collision"/>
        protected Collision Collision { get; private set; }

        /// <inheritdoc cref="Components.CinematicCamera"/>
        protected CinematicCamera CinematicCamera { get; private set; }

        /// <inheritdoc cref="Components.Motion"/>
        protected Motion Motion { get; private set; }

        /// <summary>Train that is parent of this component.</summary>
        protected Train Train { get; }

        public TrainComponent(ComponentCollection components) : base(components)
        {
            Train = GetParent<Train>();
        }

        public override void Start()
        {
            Physx = Components.GetComponent<Physx>();
            Sounds = Components.GetComponent<Sounds>();
            Particles = Components.GetComponent<Particles>();
            Driving = Components.GetComponent<Driving>();
            Derail = Components.GetComponent<Derail>();
            Collision = Components.GetComponent<Collision>();
            CinematicCamera = Components.GetComponent<CinematicCamera>();
            Motion = Components.GetComponent<Motion>();
        }
    }
}
