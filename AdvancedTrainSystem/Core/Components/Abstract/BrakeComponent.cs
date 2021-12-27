using FusionLibrary.Extensions;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core.Components.Abstract
{
    /// <summary>
    /// Defines a basic train brakes.
    /// </summary>
    public abstract class BrakeComponent : Component
    {
        /// <summary>
        /// Normalized brake force value.
        /// </summary>
        public float Force => force;

        /// <summary>
        /// How much brake affects train speed.
        /// </summary>
        public abstract float Intensity { get; }

        private float force;
        protected PhysxComponent physx;

        public BrakeComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            physx = Components.GetComponent<PhysxComponent>();
        }

        /// <summary>
        /// Applies brake force on train.
        /// </summary>
        /// <param name="force">Force to apply in range of 0.0 - 1.0. 
        /// If value is out of this range, it will be clamped.</param>
        public void Apply(float force)
        {
            this.force = force.Clamp(0f, 1f);
        }

        /// <summary>
        /// Releases train brakes.
        /// </summary>
        public void Release()
        {
            force = 0f;
        }
    }
}
