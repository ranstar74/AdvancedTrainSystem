using AdvancedTrainSystem.Train;
using GTA;

namespace AdvancedTrainSystem.Data
{
    /// <summary>
    /// Contains info about train collision.
    /// </summary>
    public class CollisionInfo
    {
        /// <summary>
        /// Carriage that collides with <see cref="CollidingVehicle"/>.
        /// </summary>
        public readonly Carriage Carriage;

        /// <summary>
        /// Vehicle that collides with <see cref="Carriage"/>.
        /// </summary>
        public readonly Vehicle CollidingVehicle;

        /// <summary>
        /// Constructs new instance of <see cref="CollisionInfo"/>.
        /// </summary>
        /// <param name="carriage"></param>
        /// <param name="collidingVehicle"></param>
        public CollisionInfo(Carriage carriage, Vehicle collidingVehicle)
        {
            Carriage = carriage;
            CollidingVehicle = collidingVehicle;
        }
    }
}
