using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Railroad.Components;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad
{
    /// <summary>This class defines a steam train.</summary>
    public class SteamTrain : Train
    {
        /// <summary>Components of the <see cref="SteamTrain"/>.</summary>
        public new SteamTrainComponentCollection Components => (SteamTrainComponentCollection) GetComponents();

        /// <summary>Components of the <see cref="SteamTrain"/>.</summary>
        protected SteamTrainComponentCollection components;

        internal SteamTrain(TrainSpawnData spawnData) : base(spawnData)
        {
            components = new SteamTrainComponentCollection(this);

        }

        /// <summary>Gets the <see cref="TrainComponentCollection"/>.</summary>
        /// <returns>A <see cref="TrainComponentCollection"/> instance of the <see cref="Train"/></returns>
        public override ComponentCollection GetComponents()
        {
            return components;
        }
    }
}
