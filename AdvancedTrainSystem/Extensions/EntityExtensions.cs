using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Train;
using FusionLibrary.Extensions;
using GTA;

namespace AdvancedTrainSystem.Extensions
{
    /// <summary>
    /// Various <see cref="Entity"/> extensions.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Checks if vehicle is <see cref="CustomTrain"/>.
        /// </summary>
        /// <param name="vehicle">Vehicle to check.</param>
        /// <returns></returns>
        public static bool IsCustomTrain(this Vehicle vehicle)
        {
            var headHandle = vehicle.Decorator().GetInt(Constants.TrainHeadHandle);

            return headHandle != 0;
        }
    }
}
