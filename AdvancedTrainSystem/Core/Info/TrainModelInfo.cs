using AdvancedTrainSystem.Core.Abstract;
using FusionLibrary;
using System;

namespace AdvancedTrainSystem.Core.Info
{
    /// <summary>
    /// Contains model set for <see cref="Carriage"/>.
    /// </summary>
    public class TrainModelInfo
    {
        /// <summary>
        /// <see cref="CustomModel"/> of <see cref="Carriage.HiddenVehicle"/>.
        /// </summary>
        public string HiddenVehicleModel { get; set; }

        /// <summary>
        /// <see cref="CustomModel"/> of <see cref="Carriage.Vehicle"/>.
        /// </summary>
        public string VehicleModel { get; set; }
    }
}
