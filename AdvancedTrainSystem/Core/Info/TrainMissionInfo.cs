using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core.Info
{
    /// <summary>
    /// This is an wrapper for game TrainConfig.
    /// <para>
    /// It contains Id of config item, list of used models and display name.
    /// </para>
    /// </summary>
    [Serializable]
    public struct TrainMissionInfo
    {
        /// <summary>
        /// Id of the train config.
        /// </summary>
        public int Id;

        /// <summary>
        /// Models of the train config. All models must be loaded before spawning train.
        /// </summary>
        public List<TrainModelInfo> Models;

        /// <summary>
        /// Creates a new instance of <see cref="TrainMissionInfo"/>.
        /// </summary>
        /// <param name="id">Id of the train config.</param>
        /// <param name="models">List of models.</param>
        public TrainMissionInfo(int id, List<TrainModelInfo> models)
        {
            Id = id;
            Models = models;
        }
    }
}
