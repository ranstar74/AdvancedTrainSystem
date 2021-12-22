using GTA;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Advanced train with simulation of many components and physic behaviour.
    /// </summary>
    public class TrainLocomotive : Carriage
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
        /// <param name="hiddenVehicle">Hidden vehicle of the <see cref="Carriage"/></param>
        /// <param name="vehicle">Visible vehicle of the <see cref="Carriage"/></param>
        internal TrainLocomotive(Vehicle hiddenVehicle, Vehicle vehicle) : base(hiddenVehicle, vehicle)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="TrainLocomotive"/> from <see cref="Carriage"/>.
        /// </summary>
        /// <param name="carriage"><see cref="Carriage"/> instance to get vehicles from.</param>
        internal TrainLocomotive(Carriage carriage) : base(carriage.HiddenVehicle, carriage.Vehicle)
        {

        }
    }
}
