using AdvancedTrainSystem.Core.Data;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core
{
    internal struct TrainSpawnData
    {
        public TrainData TrainInfo { get; }

        public TrainLocomotive Locomotive { get; }

        public List<TrainCarriage> Carriages { get; }

        public bool Direction { get; }

        public TrainSpawnData(
            TrainData trainInfo, 
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
