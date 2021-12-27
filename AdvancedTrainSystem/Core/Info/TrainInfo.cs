using AdvancedTrainSystem.Railroad.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AdvancedTrainSystem.Core.Info
{
    /// <summary>Contains train configuration that is used for creating one.</summary>
    /// <remarks>It is not recommened to manually create this structure, as
    /// after reload script should re-read this config to fully
    /// recover the train structure.</remarks>
    public class TrainInfo
    {
        /// <summary>Current version of this config.
        /// <para>Needs to be increased after doing any changes.</para>
        /// </summary>
        public static readonly Version CurrentVersion = new Version(1, 0);

        /// <summary>Version this config is made for.</summary>
        public Version Version { get; set; }

        /// <summary>Display name of train.</summary>
        public string Name { get; set; }

        /// <summary>Type of this train.</summary>
        public TrainType TrainType { get; set; }

        /// <summary>A Collection of the FMOD sound bank names used by train.</summary>
        public IEnumerable<string> SoundBanks { get; set; }

        /// <summary>GTA Train mission infromation of this train.</summary>
        public TrainMissionInfo TrainMissionInfo { get; set; }

        /// <summary>A Collection of interactive train controls.</summary>
        public IEnumerable<TrainControlBehaviourInfo> ControlBehaviourInfos { get; set; }

        /// <summary>System directory, where all configs are stored.</summary>
        public string ConfigDirectory => _configDirectory;

        private const string _configDirectory = "scripts/ATS/Configs/";

        /// <summary>Reads a <see cref="TrainInfo"/> by config name.</summary>
        /// <param name="name">Name of the config to read.</param>
        /// <returns>A new <see cref="TrainInfo"/> instance with loaded config.</returns>
        public static TrainInfo Load(string name)
        {
            // In case if name already contains .json in it
            string path = name.Contains(".json") ? name : name + ".json";
            path = _configDirectory + path;

            if (!File.Exists(path))
                throw new Exception($"Config with name: {name} cannot be found.");

            string json = File.ReadAllText(path);

            return EnsureValid(JsonConvert.DeserializeObject<TrainInfo>(json));
        }

        /// <summary>Loads train config by mission id.</summary>
        /// <param name="missionId">Id of the train mission.</param>
        /// <returns>A new <see cref="TrainInfo"/> instance with loaded config.</returns>
        public static TrainInfo Load(int missionId)
        {
            string[] files = Directory.GetFiles(_configDirectory);

            foreach (string file in files)
            {
                TrainInfo trainInfo = LoadFromFile(file);
                if (trainInfo.TrainMissionInfo.Id == missionId)
                    return trainInfo;
            }
            throw new Exception($"Config with mission id: {missionId} cannot be found.");
        }

        /// <summary>Gets all train configs from <see cref="ConfigDirectory"/>.</summary>
        /// <returns>A Collection, containing all readed configs.</returns>
        public static IEnumerable<TrainInfo> GetAllConfigs()
        {
            string[] files = Directory.GetFiles(_configDirectory);

            foreach (string file in files)
            {
                yield return LoadFromFile(file);
            }
        }

        private static TrainInfo LoadFromFile(string file)
        {
            TrainInfo trainInfo = Load(Path.GetFileNameWithoutExtension(file));
            return EnsureValid(trainInfo);
        }

        private static TrainInfo EnsureValid(TrainInfo trainInfo)
        {
            if (trainInfo.Version != CurrentVersion)
                throw new Exception(
                    $"Version of the config: {trainInfo.Version} is not compatible with current version: {CurrentVersion}");

            return trainInfo;
        }
    }
}
