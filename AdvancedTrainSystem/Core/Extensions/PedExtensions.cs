using AdvancedTrainSystem.Extensions;
using GTA;

namespace AdvancedTrainSystem.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Ped"/>.
    /// </summary>
    public static class PedExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="Ped"/> is in the <see cref="TrainLocomotive"/>.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns>True if <see cref="Ped"/> is in the <see cref="Train"/>, otherwise False.</returns>
        public static bool IsInAdvancedTrain(this Ped ped)
        {
            return ped.CurrentVehicle?.IsAts() == true;
        }
    }
}
