using AdvancedTrainSystem.Core.Info;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Contains information required to create <see cref="Train"/> instance.
    /// </summary>
    internal struct TrainSpawnData
    {
        public TrainInfo TrainInfo { get; }

        public TrainLocomotive Locomotive { get; }

        public List<TrainCarriage> Carriages { get; }

        public bool Direction { get; }

        public TrainSpawnData(
            TrainInfo trainInfo, 
            TrainLocomotive locomotive, 
            List<TrainCarriage> carriages, 
            bool direction)
        {
            TrainInfo = trainInfo;
            Locomotive = locomotive;
            Carriages = carriages;
            Direction = direction;
        }
    }
}
