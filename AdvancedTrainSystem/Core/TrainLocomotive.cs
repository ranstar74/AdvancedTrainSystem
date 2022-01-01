using GTA;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Advanced train with simulation of many components and physic behaviour.
    /// </summary>
    public class TrainLocomotive : TrainCarriage
    {
        /// <summary>
        /// Gets a <see cref="Ped"/> that is currently driving the <see cref="TrainLocomotive"/>.
        /// </summary>
        public Ped Driver => HiddenVehicle.Driver;

        /// <summary>
        /// Vehicle of the <see cref="TrainLocomotive"/>.
        /// </summary>
        public new Vehicle Vehicle => this;

        /// <summary>
        /// Creates a new instance of <see cref="TrainLocomotive"/> from existing vehicles.
        /// </summary>
        /// <param name="hiddenVehicle">Hidden vehicle of the <see cref="TrainCarriage"/></param>
        /// <param name="vehicle">Visible vehicle of the <see cref="TrainCarriage"/></param>
        internal TrainLocomotive(Vehicle hiddenVehicle, Vehicle vehicle) : base(hiddenVehicle, vehicle)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="TrainLocomotive"/> from <see cref="TrainCarriage"/>.
        /// </summary>
        /// <param name="carriage"><see cref="TrainCarriage"/> instance to get vehicles from.</param>
        internal TrainLocomotive(TrainCarriage carriage) : base(carriage.HiddenVehicle, carriage.Vehicle)
        {

        }
    }
}
