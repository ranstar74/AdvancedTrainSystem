using FusionLibrary.Extensions;
using RageComponent;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles train brakes.
    /// </summary>
    public class BrakeComponent : Component<CustomTrain>
    {
        /// <summary>
        /// Current level of airbrake. 0 - no brake, 1 - full brake.
        /// </summary>
        public float AirbrakeForce { get; internal set; }

        /// <summary>
        /// Full brake blocks any wheel movement.
        /// </summary>
        public int FullBrakeForce { get; internal set; }
    }
}
