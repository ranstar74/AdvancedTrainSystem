using GTA;
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
        public float AirbrakeForce { get; set; }

        /// <summary>
        /// Steam brake blocks any wheel movement.
        /// </summary>
        public int SteamBrake { get; set; }
    }
}
