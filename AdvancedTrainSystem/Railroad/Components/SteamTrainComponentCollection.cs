﻿using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Railroad.Components.SteamComponents;
using AdvancedTrainSystem.Railroad.SharedComponents;

namespace AdvancedTrainSystem.Railroad.Components
{
    /// <summary>
    /// This class contains all <see cref="SteamTrain"/> components.
    /// </summary>
    public class SteamTrainComponentCollection : TrainComponentCollection
    {
        public BoilerComponent Boiler;
        public DynamoComponent Generator;
        public LightComponent Light;

        /// <summary>
        /// Creates a new instance of <see cref="SteamTrainComponentCollection"/>.
        /// </summary>
        /// <param name="train"></param>
        public SteamTrainComponentCollection(SteamTrain train) : base(train)
        {
            Boiler = Create<BoilerComponent>();
            Generator = Create<DynamoComponent>();
            Light = Create<LightComponent>();

            OnStart();
        }
    }
}
