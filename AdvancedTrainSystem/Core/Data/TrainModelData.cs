using FusionLibrary;

namespace AdvancedTrainSystem.Core.Data
{
    /// <summary>
    /// Contains model set for <see cref="TrainCarriage"/>.
    /// </summary>
    public class TrainModelData
    {
        /// <summary>
        /// <see cref="CustomModel"/> of <see cref="TrainCarriage.HiddenVehicle"/>.
        /// </summary>
        public string HiddenVehicleModel { get; set; }

        /// <summary>
        /// <see cref="CustomModel"/> of <see cref="TrainCarriage.Vehicle"/>.
        /// </summary>
        public string VehicleModel { get; set; }
    }
}
