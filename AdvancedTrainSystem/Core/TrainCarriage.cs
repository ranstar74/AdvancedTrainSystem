using GTA;
using Newtonsoft.Json;
using System;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Defines an carriage that consists of a invisible train and visible vehicle.
    /// <list type="bullet">
    ///     <item>
    ///         Invisible vehicle is an actual train moving on spline,
    ///         usually its a low poly model.
    ///     </item>
    ///     <item>
    ///         Vehicle that is attached to <see cref="HiddenVehicle"/>, so it uses <see cref="HiddenVehicle"/>
    ///         only for position and rotation.
    ///     </item>
    /// </list>
    /// </summary>
    public class TrainCarriage : IDisposable
    {
        /// <summary>
        /// Invisible vehicle of the <see cref="TrainCarriage"/>.
        /// </summary>
        internal Vehicle HiddenVehicle => _hiddenVehicle;

        /// <summary>
        /// Visible vehicle of the <see cref="TrainCarriage"/>.
        /// </summary>
        internal Vehicle Vehicle => _vehicle;

#pragma warning disable IDE0052 // Remove unread private members
        [JsonProperty("HiddenVehicleHandle")]
        private int HiddenVehicleHandle
        {
            get => _hiddenVehicle.Handle;
            set
            {
                _hiddenVehicle = (Vehicle) Entity.FromHandle(value);
            }
        }

        [JsonProperty("VehicleHandle")]
        private int VehicleHandle
        {
            get => _vehicle.Handle;
            set
            {
                _vehicle = (Vehicle)Entity.FromHandle(value);
            }
        }
#pragma warning restore IDE0052 // Remove unread private members

        private Vehicle _hiddenVehicle;
        private Vehicle _vehicle;

        /// <summary>
        /// Creates a new instance of <see cref="TrainCarriage"/> with given vehicles.
        /// </summary>
        internal TrainCarriage(Vehicle hiddenVehicle, Vehicle vehicle)
        {
            _hiddenVehicle = hiddenVehicle;
            _vehicle = vehicle;

            HiddenVehicleHandle = hiddenVehicle.Handle;
            VehicleHandle = vehicle.Handle;
        }

        // JSON Constructor
        private TrainCarriage()
        {

        }

        /// <summary>
        /// Disposes the <see cref="TrainCarriage"/>.
        /// </summary>
        public void Dispose()
        {
            Vehicle.Delete();
            HiddenVehicle.Delete();
        }

        /// <summary>
        /// Implicitely casts the <see cref="Train"/> to a <see cref="Entity"/>.
        /// </summary>
        /// <param name="carriage">Source object.</param>
        public static implicit operator Entity(TrainCarriage carriage) => carriage.Vehicle;

        /// <summary>
        /// Implicitely casts the <see cref="Train"/> to a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="carriage">Source object.</param>
        public static implicit operator Vehicle(TrainCarriage carriage) => carriage.Vehicle;
    }
}
