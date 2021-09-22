using GTA;

namespace AdvancedTrainSystem.Train
{
    /// <summary>
    /// Train carriage.
    /// </summary>
    public class Carriage
    {
        /// <summary>
        /// <see cref="CustomTrain"/> this carriage belongs to.
        /// </summary>
        public CustomTrain CustomTrain { get; set; }

        /// <summary>
        /// Invisible vehicle of carriage. Used only for position and rotation.
        /// </summary>
        public Vehicle InvisibleVehicle { get; set; }

        /// <summary>
        /// Visible vehicle of carriage.
        /// </summary>
        public Vehicle VisibleVehicle { get; set; }

        /// <summary>
        /// Carriage that is next to this one (towards front). For tender it will be locomotive.
        /// </summary>
        public Carriage Next { get; set; }

        /// <summary>
        /// Carriage that is behind this one (towards back). For locomotive it will be tender.
        /// </summary>
        public Carriage Previous { get; set; }

        /// <summary>
        /// Whether carriage is coupled with <see cref="Next"/> carriage or not.
        /// </summary>
        public bool CoupledWithNext { get; set; }

        /// <summary>
        /// Whether carriage is coupled with <see cref="Previous"/> carriage or not.
        /// </summary>
        public bool CoupleWithPrevious { get; set; }

        /// <summary>
        /// Constructs new instance of <see cref="Carriage"/>.
        /// </summary>
        /// <param name="invisibleVehicle"><paramref name="invisibleVehicle"/></param>
        /// <param name="visibleVehicle"><paramref name="visibleVehicle"/></param>
        public Carriage(Vehicle invisibleVehicle, Vehicle visibleVehicle)
        {
            InvisibleVehicle = invisibleVehicle; 
            VisibleVehicle = visibleVehicle;
        }

        /// <summary>
        /// Decouples carriage from the rest of the train.
        /// </summary>
        /// <param name="next">If True, carriage will be decoupled from <see cref="Next"/> carriage,
        /// otherwise from <see cref="Previous"/></param>
        public void Decouple(bool next)
        {
            VisibleVehicle.Detach();
            VisibleVehicle.IsPositionFrozen = true;
        }
    }
}
