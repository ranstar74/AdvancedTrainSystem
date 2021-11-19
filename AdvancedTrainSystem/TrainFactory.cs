using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Info;
using AdvancedTrainSystem.Railroad;
using AdvancedTrainSystem.Railroad.Enums;
using GTA.Math;
using System;

namespace AdvancedTrainSystem
{
    /// <summary>
    /// Methods for creating advanced trains.
    /// </summary>
    public static class TrainFactory
    {
        /// <summary>
        /// Creates a new train from given config on coordinates with direction.
        /// </summary>
        /// <param name="trainInfo">Config to create train from.</param>
        /// <param name="position">Position where to spawn train.</param>
        /// <param name="direction">Direction of train on tracks.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Currently will be thrown for all except SteamTrain.</exception>
        /// <exception cref="NotSupportedException">Will be thrown if unknown train type is passed.</exception>
        public static Train CreateTrain(TrainInfo trainInfo, Vector3 position, bool direction)
        {
            Train train;

            switch(trainInfo.TrainType)
            {
                case TrainType.Steam:
                    train = Train.Create<SteamTrain>(trainInfo, position, direction);
                    break;
                case TrainType.Diesel:
                    throw new NotImplementedException();
                case TrainType.Electric:
                    throw new NotImplementedException();
                case TrainType.Handcar:
                    throw new NotImplementedException();
                case TrainType.Minecart:
                    throw new NotImplementedException();
                default: 
                    throw new NotSupportedException();
            }

            ATSPool.Trains.Add(train);

            return train;
        }
    }
}
