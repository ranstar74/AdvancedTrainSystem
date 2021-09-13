using System.Collections.Generic;

namespace RogersSierra.Train
{
    public class TrainConfig
    {
        /// <summary>
        /// Id of the train config. Could be changed before spawning train.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of train blip. Leave empty if if not required.
        /// </summary>
        public string BlipName {  get; set; }

        /// <summary>
        /// Models of the train config. All models must be loaded before spawning train.
        /// </summary>
        public List<TrainModel> Models { get; set; }

        public TrainConfig(int id, List<TrainModel> models, string blipName = "")
        {
            Id = id;
            Models = models;
            BlipName = blipName;
        }
    }
}
