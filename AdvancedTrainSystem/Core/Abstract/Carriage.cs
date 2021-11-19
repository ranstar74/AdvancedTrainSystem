using GTA;
using System;

namespace AdvancedTrainSystem.Core.Abstract
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
    public abstract class Carriage : IDisposable
    {
        /// <summary>
        /// Invisible vehicle of the <see cref="Carriage"/>.
        /// </summary>
        internal Vehicle HiddenVehicle { get; }

        /// <summary>
        /// Visible vehicle of the <see cref="Carriage"/>.
        /// </summary>
        internal Vehicle Vehicle { get; }

        /// <summary>
        /// <see cref="Train"/> the <see cref="Carriage"/> attached to.
        /// </summary>
        public Train Train => train;

        /// <summary>
        /// Gets passengers of the <see cref="TrainLocomotive"/>.
        /// </summary>
        public Ped[] Passengers => HiddenVehicle.Passengers;

        private Train train;

        /// <summary>
        /// Creates a new instance of <see cref="Carriage"/> with given vehicles.
        /// </summary>
        internal Carriage(Vehicle hiddenVehicle, Vehicle vehicle)
        {
            HiddenVehicle = hiddenVehicle;
            Vehicle = vehicle;
        }

        /// <summary>
        /// Sets <see cref="Train"/> of the <see cref="TrainCarriage"/>.
        /// </summary>
        /// <param name="train"></param>
        internal void SetTrain(Train train)
        {
            this.train = train;
        }

        /// <summary>
        /// Disposes the <see cref="Carriage"/>.
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
        public static implicit operator Entity(Carriage carriage) => carriage.Vehicle;

        /// <summary>
        /// Implicitely casts the <see cref="Train"/> to a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="carriage">Source object.</param>
        public static implicit operator Vehicle(Carriage carriage) => carriage.Vehicle;
    }
}
