using AdvancedTrainSystem.Core.Components.Enums;

namespace AdvancedTrainSystem.Railroad.Interfaces
{
    /// <summary>
    /// Defines a train that have lights.
    /// </summary>
    public interface IHasLight
    {
        /// <summary>
        /// State of the train lights.
        /// </summary>
        LightState LightState { get; set; }
    }
}
