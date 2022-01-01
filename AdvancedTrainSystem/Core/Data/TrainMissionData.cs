using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core.Data
{
    /// <summary>
    /// This is an wrapper for game TrainConfig.
    /// <para>
    /// It contains Id of config item, list of used models and display name.
    /// </para>
    /// </summary>
    public class TrainMissionData
    {
        /// <summary>
        /// Id of the train config.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Models of the train config. All models must be loaded before spawning train.
        /// </summary>
        public List<TrainModelData> Models { get; set; }
    }
}
