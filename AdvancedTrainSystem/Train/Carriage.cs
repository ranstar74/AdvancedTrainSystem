using GTA;

namespace RogersSierra.Train
{
    /// <summary>
    /// Train carriage.
    /// </summary>
    public class Carriage
    {
        /// <summary>
        /// Invisible vehicle of carriage. Used only for position and rotation.
        /// </summary>
        public Vehicle InvisibleVehicle { get; set; }

        /// <summary>
        /// Visible vehicle of carriage.
        /// </summary>
        public Vehicle VisibleVehicle { get; set; }

        /// <summary>
        /// Whether train is derailed or not.
        /// </summary>
        public bool IsDerailed { get; set; }

        /// <summary>
        /// Constructs new instance of <see cref="Carriage"/>.
        /// </summary>
        /// <param name="invisibleVehicle"><paramref name="InvisibleVehicle"/></param>
        /// <param name="visibleVehicle"><paramref name="visibleVehicle"/></param>
        public Carriage(Vehicle invisibleVehicle, Vehicle visibleVehicle)
        {
            InvisibleVehicle = invisibleVehicle; 
            VisibleVehicle = visibleVehicle;
        }

        /// <summary>
        /// Derails carriage and all carriages after it.
        /// </summary>
        public void Derail()
        {
            
        }

        /// <summary>
        /// Decouples carriage from the rest of the train.
        /// </summary>
        public void Decouple()
        {

        }
    }
}
