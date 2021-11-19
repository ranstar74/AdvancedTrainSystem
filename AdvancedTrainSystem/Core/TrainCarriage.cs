using AdvancedTrainSystem.Core.Abstract;
using GTA;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Train carriage.
    /// </summary>
    public class TrainCarriage : Carriage
    {
        /// <summary>
        /// Vehicle of the <see cref="Carriage"/>.
        /// </summary>
        public new Vehicle Vehicle => base.Vehicle;

        /// <summary>
        /// Constructs new instance of <see cref="Carriage"/>.
        /// </summary>
        /// <param name="hiddenVehicle">Invisible vehicle of the <see cref="Carriage"/></param>
        /// <param name="vehicle">Visible vehicle of the <see cref="Carriage"/></param>
        public TrainCarriage(Vehicle hiddenVehicle, Vehicle vehicle) : base(hiddenVehicle, vehicle)
        {

        }
    }
}
