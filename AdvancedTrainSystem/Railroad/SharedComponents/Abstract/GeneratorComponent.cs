using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.SharedComponents.Abstract
{
    /// <summary>
    /// Defines a base electricity generator component.
    /// </summary>
    public abstract class GeneratorComponent : Component
    {
        /// <summary>
        /// Gets a normalized value indicating generator output.
        /// </summary>
        public abstract float Output { get; }

        public GeneratorComponent(ComponentCollection components) : base(components)
        {

        }
    }
}
