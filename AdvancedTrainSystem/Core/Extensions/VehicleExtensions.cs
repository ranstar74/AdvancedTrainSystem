using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Railroad.Enums;
using FusionLibrary.Extensions;
using GTA;

namespace AdvancedTrainSystem.Extensions
{
    /// <summary>
    /// <see cref="Vehicle"/> extension methods.
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Checks if vehicle is a <see cref="Train"/>.
        /// </summary>
        /// <param name="vehicle">Vehicle to check.</param>
        /// <returns></returns>
        public static bool IsAts(this Vehicle vehicle)
        {
            // Valid handle starts from 1
            return vehicle.GetAtsHandle() >= 1;
        }

        public static bool IsAtsHead(this Vehicle vehicle)
        {
            return vehicle.Handle == vehicle.GetAtsHeadVehicleHandle();
        }

        /// <summary>
        /// Gets <see cref="Train"/> handle.
        /// </summary>
        /// <param name="vehicle"><see cref="Vehicle"/> to get handle from.</param>
        /// <returns><see cref="Train"/> handle if <see cref="Vehicle"/> is a <see cref="Train"/>, otherwise -1.</returns>
        public static int GetAtsHandle(this Vehicle vehicle)
        {
            return vehicle.Decorator().GetInt(TrainConstants.TrainHandle);
        }

        public static int GetAtsCarriagesCount(this Vehicle vehicle)
        {
            return vehicle.Decorator().GetInt(TrainConstants.CarriagesNumber);
        }

        public static bool GetAtsDirection(this Vehicle vehicle)
        {
            return vehicle.Decorator().GetBool(TrainConstants.TrainDirection);
        }

        public static int GetAdvancedTrainMissionId(this Vehicle vehicle)
        {
            return vehicle.Decorator().GetInt(TrainConstants.TrainMissionId);
        }

        public static int GetAtsHeadVehicleHandle(this Vehicle vehicle)
        {
            return vehicle.Decorator().GetInt(TrainConstants.TrainHeadHandle);
        }

        public static bool IsAtsDerailed(this Vehicle vehicle)
        {
            return vehicle.Decorator().GetBool(TrainConstants.IsDerailed);
        }

        public static bool IsAtsDerailed(this Train train)
        {
            return IsAtsDerailed((Vehicle) train);
        }

        /// <summary>
        /// Returns the <see cref="TrainCarriage.Vehicle"/> of <see cref="TrainCarriage.HiddenVehicle"/>.
        /// </summary>
        /// <param name="vehicle">Hidden carriage context.</param>
        /// <returns>A <see cref="Vehicle"/> instance that is attached to <see cref="TrainCarriage.HiddenVehicle"/>.</returns>
        public static Vehicle GetAtsCarriageVehicle(this Vehicle vehicle)
        {
            return (Vehicle) Entity.FromHandle(vehicle.Decorator().GetInt(TrainConstants.TrainVisibleCarriageHandle));
        }

        public static TrainType GetAtsType(this Vehicle vehicle)
        {
            return (TrainType) vehicle.Decorator().GetInt(TrainConstants.TrainType);
        }

        /// <summary>
        /// Finds a <see cref="Train"/> by <see cref="Vehicle"/> of a <see cref="TrainCarriage"/>.
        /// </summary>
        /// <param name="carriage"><see cref="TrainCarriage"/> of a <see cref="Train"/>.</param>
        /// <returns><see cref="Train"/>, <see cref="TrainCarriage"/> attached to.</returns>
        public static Train GetAtsByCarriage(this Vehicle carriage)
        {
            int handle = carriage.GetAtsHandle();

            return TrainPool.Trains.GetByHandle(handle);
        }
    }
}
