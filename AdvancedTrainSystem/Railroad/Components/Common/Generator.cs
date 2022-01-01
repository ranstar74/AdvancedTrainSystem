using AdvancedTrainSystem.Core.Components;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.Common
{
    /// <summary>An electricity generator with output.</summary>
    public abstract class Generator : TrainComponent
    {
        /// <summary>Gets a normalized value indicating generator output.</summary>
        public abstract float Output { get; }

        public Generator(ComponentCollection components) : base(components)
        {

        }
    }
}
