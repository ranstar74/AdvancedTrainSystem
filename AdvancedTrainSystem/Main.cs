using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Missions;
using AdvancedTrainSystem.Railroad;
using AdvancedTrainSystem.UI;
using FusionLibrary;
using GTA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AdvancedTrainSystem
{
    /// <summary>
    /// Main class of ATS.
    /// </summary>
    public class Main : Script
    {
        private Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        private bool _firstTick = true;
        private static readonly DateTime _gtaLaunchTime;

        static Main()
        {
            _gtaLaunchTime = Process.GetCurrentProcess().StartTime;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Main"/>.
        /// </summary>
        public Main()
        {
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);
            File.AppendAllText($"./ScriptHookVDotNet.log", $"AdvancedTrainSystem - {Version} ({buildDate})" + Environment.NewLine);

            Tick += Update;
            KeyDown += OnKeyDown;
            Aborted += MainAborted;
        }

        private struct AtsData
        {
            public DateTime SessionStartTime { get; set; }
            public bool Direction { get; set; }
            public List<Carriage> Carriages { get; set; }
        }

        private const string _atsData = "AtsData.json";
        private void MainAborted(object sender, EventArgs e)
        {
            AtsStoryMgr.Instance.Abort();

            // Check if there's train we can serialize
            if (ATSPool.Trains.Count() == 0)
            {
                IEnumerable<AtsData> trains = ATSPool.Trains.Select(t => new AtsData()
                {
                    SessionStartTime = _gtaLaunchTime,
                    Direction = t.Direction,
                    Carriages = t.Carriages
                });
                string json = JsonConvert.SerializeObject(trains);

                File.WriteAllText(_atsData, json);

                // Dispose only train components and invalidate handles
                // but keep vehicles, it will help to "hook" train after reloading
                foreach (Train train in ATSPool.Trains)
                {
                    train.MarkAsNonScripted();
                    train.Components.OnReload();
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Debugging code...

            if (e.KeyCode == Keys.L)
            {
                SteamTrain train = (SteamTrain)ATSPool.Trains[0];
                train.Components.Physx.Speed += 10;
                train.Components.Controls.Throttle = 1f;
                train.Components.Controls.Gear = 1f;
            }

            if (e.KeyCode == Keys.Y)
            {
                SpawnMenu.Instance.Visible = true;
            }
        }

        private void Update(object sender, EventArgs e)
        {
            AtsStoryMgr.Instance.Update();

            if (_firstTick)
            {
                Constants.RegisterDecorators();
                ModelHandler.RequestAll();

                // Respawn trains from previous session
                try
                {
                    if (File.Exists(_atsData))
                    {
                        string json = File.ReadAllText(_atsData);
                        IEnumerable<AtsData> trains = JsonConvert.DeserializeObject<IEnumerable<AtsData>>(json);

                        foreach(AtsData data in trains)
                        {
                            if (data.SessionStartTime != _gtaLaunchTime)
                                break;

                            Train.Respawn(data.Carriages, data.Direction);
                        }

                        File.Delete(_atsData);
                    }
                }
                catch (Exception ex)
                {
                    // Data is corruped... happens
                    GTA.UI.Screen.ShowHelpText("Restore ATS Failed: " + ex.Message);
                }

                _firstTick = false;
            }

            FusionUtils.RandomTrains = false;
        }
    }
}
