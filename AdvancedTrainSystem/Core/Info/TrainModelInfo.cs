using AdvancedTrainSystem.Core.Abstract;
using FusionLibrary;
using System;

namespace AdvancedTrainSystem.Core.Info
{
    /// <summary>
    /// Contains model set for <see cref="Carriage"/>.
    /// </summary>
    [Serializable]
    public struct TrainModelInfo
    {
        /// <summary>
        /// <see cref="CustomModel"/> of <see cref="Carriage.HiddenVehicle"/>.
        /// </summary>
        public string HiddenVehicleModel;

        /// <summary>
        /// <see cref="CustomModel"/> of <see cref="Carriage.Vehicle"/>.
        /// </summary>
        public string VehicleModel;

        /// <summary>
        /// Constructs new instance of <see cref="TrainModelInfo"/>.
        /// </summary>
        /// <param name="hiddenModel">Model of the <see cref="HiddenVehicleModel"/></param>
        /// <param name="vehicleModel">Model of the <see cref="VehicleModel"/></param>
        public TrainModelInfo(CustomModel hiddenModel, CustomModel vehicleModel)
        {
            HiddenVehicleModel = hiddenModel;
            VehicleModel = vehicleModel;
        }
    }
}
