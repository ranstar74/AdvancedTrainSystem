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
        /// Whether train is derailed or not.
        /// </summary>
        public bool IsDerailed { get; set; }

        /// <summary>
        /// Carriage that is next to this one (towards front). For tender it will be locomotive.
        /// </summary>
        public Carriage Next { get; set; }

        /// <summary>
        /// Carriage that is behind this one (towards back). For locomotive it will be tender.
        /// </summary>
        public Carriage Previous { get; set; }

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

        ///// <summary>
        ///// Derails carriage and all carriages after it.
        ///// </summary>
        //public void Derail()
        //{
        //    InvisibleVehicle.IsCollisionEnabled = false;
        //    VisibleVehicle.Detach();

        //    if(Game.Player.Character.IsInVehicle(InvisibleVehicle))
        //    {
        //        Game.Player.Character.Task.WarpIntoVehicle(VisibleVehicle, Game.Player.Character.SeatIndex);
        //    }

        //    IsDerailed = true;

        //    if (Previous != null)
        //        Previous.Derail();
        //}

        /// <summary>
        /// Decouples carriage from the rest of the train.
        /// </summary>
        public void Decouple()
        {

        }
    }
}
