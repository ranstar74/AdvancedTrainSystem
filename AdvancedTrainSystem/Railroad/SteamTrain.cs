using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Info;
using AdvancedTrainSystem.Railroad.Components;
using AdvancedTrainSystem.Railroad.SharedComponents;
using AdvancedTrainSystem.Railroad.SharedComponents.Abstract;
using AdvancedTrainSystem.Railroad.SharedComponents.Interfaces;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad
{
    /// <summary>
    /// This class defines a steam train.
    /// </summary>
    public class SteamTrain : Train, IHasElectricity
    {
        /// <summary>
        /// Components of the <see cref="SteamTrain"/>.
        /// </summary>
        public new SteamTrainComponentCollection Components => (SteamTrainComponentCollection) GetComponents();

        public LightComponent LightComponent => Components.Light;

        public GeneratorComponent GeneratorComponent => Components.Generator;

        /// <summary>
        /// Components of the <see cref="SteamTrain"/>.
        /// </summary>
        protected SteamTrainComponentCollection components;

        internal SteamTrain(TrainSpawnData spawnData) : base(spawnData)
        {
            components = new SteamTrainComponentCollection(this);

        }

        /// <summary>
        /// Gets <see cref="TrainComponentCollection"/>.
        /// </summary>
        /// <returns>A <see cref="TrainComponentCollection"/> instance of the <see cref="Train"/></returns>
        public override ComponentCollection GetComponents()
        {
            return components;
        }
    }
}
