using AdvancedTrainSystem.Railroad.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace AdvancedTrainSystem.Core.Info
{
    /// <summary>
    /// Contains train configuration that is used for creating one.
    /// </summary>
    /// <remarks>
    /// It is not recommened to manually create this structure, as
    /// after reload script should re-read this config to fully
    /// recover the train structure.
    /// </remarks>
    public class TrainInfo
    {
        /// <summary>
        /// Display name of train.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of this train.
        /// </summary>
        public TrainType TrainType { get; set; }

        /// <summary>
        /// Names of the FMOD sound bank used by train.
        /// </summary>
        public List<string> SoundBanks { get; set; }

        /// <summary>
        /// GTA Train mission infromation of this train.
        /// </summary>
        public TrainMissionInfo TrainMissionInfo { get; set; }

        /// <summary>
        /// Train controls.
        /// </summary>
        public List<TrainControlBehaviourInfo> ControlBehaviourInfos { get; set; }

        private const string configDirectory = "scripts/ATS/Configs/";

        /// <summary>
        /// Reads a <see cref="TrainInfo"/> by config name.
        /// </summary>
        /// <param name="name">Name of the config to read.</param>
        /// <returns>A new <see cref="TrainInfo"/> instance.</returns>
        public static TrainInfo Load(string name)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            // In case if name already contains .json in it
            string path = name.Contains(".json") ? name : name + ".json";
            path = configDirectory + path;

            if (!File.Exists(path))
                throw new Exception($"Config with name: {name} cannot be found.");

            string json = File.ReadAllText(path);

            return serializer.Deserialize<TrainInfo>(json);
        }

        /// <summary>
        /// Reads train config by mission id.
        /// </summary>
        /// <param name="missionId">Id of the train mission.</param>
        /// <returns>A new <see cref="TrainInfo"/> instance.</returns>
        public static TrainInfo Load(int missionId)
        {
            string[] files = Directory.GetFiles(configDirectory);

            foreach(string fileName in files)
            {
                TrainInfo trainInfo = Load(fileName);

                if (trainInfo.TrainMissionInfo.Id == missionId)
                    return trainInfo;
            }
            throw new Exception($"Config with mission id: {missionId} cannot be found.");
        }
    }
}
