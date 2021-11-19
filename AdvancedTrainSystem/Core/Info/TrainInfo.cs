using AdvancedTrainSystem.Railroad.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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
    [Serializable]
    public struct TrainInfo
    {
        /// <summary>
        /// Display name of train.
        /// </summary>
        public string Name;

        /// <summary>
        /// Type of this train.
        /// </summary>
        public TrainType TrainType;

        /// <summary>
        /// Names of the FMOD sound bank used by train.
        /// </summary>
        public List<string> SoundBanks;

        /// <summary>
        /// GTA Train mission infromation of this train.
        /// </summary>
        public TrainMissionInfo TrainMissionInfo;

        /// <summary>
        /// Gets name of file this config was readed from.
        /// </summary>
        [XmlIgnore]
        public string ConfigFileName => configFileName;

        [NonSerialized]
        private string configFileName;

        private const string configPathTemplate = configDirectory + "{0}.xml";
        private const string configDirectory = "scripts/ATS/Configs/";

        /// <summary>
        /// Saves config in .XML file.
        /// </summary>
        /// <remarks>
        /// Previous config will be overwritten.
        /// </remarks>
        /// <param name="configName">Name of the file config will be saved in.</param>
        public void Save(string configName)
        {
            Directory.CreateDirectory(configDirectory);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TrainInfo));

            string path = string.Format(configPathTemplate, configName);

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, this);
            }
        }

        /// <summary>
        /// Reads train config from .XML file from given name.
        /// </summary>
        /// <param name="configName">Config name to read.</param>
        /// <returns>A new <see cref="TrainInfo"/> instance .</returns>
        public static TrainInfo Load(string configName)
        {
            string path = string.Format(configPathTemplate, configName);

            return LoadFromFile(path);
        }

        /// <summary>
        /// Reads train config from .XML file on given path.
        /// </summary>
        /// <param name="fileName">Path where config is located.</param>
        /// <returns>A new <see cref="TrainInfo"/> instance.</returns>
        private static TrainInfo LoadFromFile(string fileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TrainInfo));

            TrainInfo result;
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                result = (TrainInfo)xmlSerializer.Deserialize(fs);

                result.configFileName = Path.GetFileName(fs.Name);
            }

            return result;
        }

        public static TrainInfo Load(int missionId)
        {
            // Not sure if this is the best way
            // We're basically looking through all config
            // and checking if id matches

            string[] files = Directory.GetFiles(configDirectory);

            foreach(string file in files)
            {
                TrainInfo trainInfo = LoadFromFile(file);

                if (trainInfo.TrainMissionInfo.Id == missionId)
                    return trainInfo;
            }
            throw new Exception($"Config with mission id: {missionId} cannot be found.");
        }
    }
}
