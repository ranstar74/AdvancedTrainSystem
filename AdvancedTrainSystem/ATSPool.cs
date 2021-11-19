using AdvancedTrainSystem.Core;
using RageComponent.Core;

namespace AdvancedTrainSystem
{
    /// <summary>
    /// This class contains <see cref="AdvancedTrainSystem"/> object pool's.
    /// </summary>
    public static class ATSPool
    {
        /// <summary>
        /// <see cref="Train"/> pool.
        /// </summary>
        public static ComponentObjectPool Trains => _trains;

        private static readonly ComponentObjectPool _trains = new ComponentObjectPool();
    }
}
