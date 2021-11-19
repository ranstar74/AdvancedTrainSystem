using FusionLibrary.Extensions;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.Abstract
{
    public abstract class BrakeComponent : Component
    {
        /// <summary>
        /// Gets a normalized value indicating how much brake is applied.
        /// </summary>
        public float BrakeForce
        {
            get => brakeForce;
            set
            {
                brakeForce = value.Clamp(0, 1);
            }
        }

        /// <summary>
        /// How much brake affects train speed.
        /// <para>
        /// Setting <see cref="float.MaxValue"/> will instantly stop the train if <see cref="BrakeForce"/> is not set to 0f.
        /// </para>
        /// </summary>
        public float BrakeEffeciency { get; private set; }

        private float brakeForce;

        /// <summary>
        /// Creates a new instance of <see cref="BrakeComponent"/>.
        /// </summary>
        /// <param name="components"></param>
        public BrakeComponent(ComponentCollection components) : base(components)
        {

        }
    }
}
