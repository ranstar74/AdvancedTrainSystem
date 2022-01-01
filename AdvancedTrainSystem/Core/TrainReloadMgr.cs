using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdvancedTrainSystem.Core
{
    internal static class TrainReloadMgr
    {
        private const string _atsTempFile = "ats_reload.temp";
        private static readonly DateTime _gtaLaunchTime;

        static TrainReloadMgr()
        {
            _gtaLaunchTime = Process.GetCurrentProcess().StartTime;
        }

        private struct AtsData
        {
            public DateTime SessionStartTime { get; set; }
            public bool Direction { get; set; }
            public List<TrainCarriage> Carriages { get; set; }
        }

        public static void SaveTrains()
        {
            List<Train> poolTrains = TrainPool.Trains.ToList();

            // Check if there's train we can serialize
            if (poolTrains.Count == 0)
            {
                return;
            }

            IEnumerable<AtsData> saveTrains = poolTrains.Select(t => new AtsData()
            {
                SessionStartTime = _gtaLaunchTime,
                Direction = t.Direction,
                Carriages = t.Carriages
            });
            string json = JsonConvert.SerializeObject(saveTrains);
            
            File.WriteAllText(_atsTempFile, json);

            // Dispose only train components
            // but keep vehicles, it will help to "hook" train after reloading
            foreach (Train train in poolTrains)
            {
                train.MarkAsNonScripted();
                train.Components.OnReload();
            }
        }

        public static void LoadTrains()
        {
            try
            {
                if (File.Exists(_atsTempFile))
                {
                    string json = File.ReadAllText(_atsTempFile);
                    IEnumerable<AtsData> trains = JsonConvert.DeserializeObject<IEnumerable<AtsData>>(json);

                    foreach (AtsData data in trains)
                    {
                        if (data.SessionStartTime != _gtaLaunchTime)
                            break;

                        Train.Respawn(data.Carriages, data.Direction);
                    }

                    File.Delete(_atsTempFile);
                }
            }
            catch (Exception ex)
            {
                // Data is corruped... happens
                GTA.UI.Screen.ShowHelpText("Restore ATS Failed: " + ex.Message);
            }
        }
    }
}
