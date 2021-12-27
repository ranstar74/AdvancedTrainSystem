using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Info;
using AdvancedTrainSystem.Extensions;
using AdvancedTrainSystem.GUI;
using AdvancedTrainSystem.Railroad;
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
            // Nothing to dispose and serialize
            if (ATSPool.Trains.Count() == 0)
                return;

            // Dispose only train components and invalidate handles
            // but keep vehicles, it will help to "hook" train after reloading
            foreach(Train train in ATSPool.Trains)
            {
                train.MarkAsNonScripted();
                train.Components.OnReload();
            }

            IEnumerable<AtsData> trains = ATSPool.Trains.Select(t => new AtsData()
            {
                SessionStartTime = _gtaLaunchTime,
                Direction = t.Direction,
                Carriages = t.Carriages
            });
            string json = JsonConvert.SerializeObject(trains);

            File.WriteAllText(_atsData, json);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Y)
            {
                // Debugging code...
                SpawnMenu.Instance.Visible = !SpawnMenu.Instance.Visible;
                
                //SteamTrain train = (SteamTrain) ATSPool.Trains[0];
                //train.Components.Physx.Speed += 10;
                //train.Components.Controls.Throttle = 1f;
                //train.Components.Controls.Gear = 1f;

                //Train train = ATSPool.Trains[0] as Train;

                //string json = ;

                //List<Carriage> carriages = JsonConvert.DeserializeObject<List<Carriage>>(json);

                //GTA.UI.Screen.ShowSubtitle(carriages.First().HiddenVehicle.Position.ToString());

                //var veh = Game.Player.Character.CurrentVehicle;
                //if (veh != null)
                //{
                //    GTA.UI.Screen.ShowSubtitle("Requesting...");
                //    string animDict = "anim@veh@sierra";
                //    string animName = "front_wheels_move";

                //    Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

                //    var endtime = DateTime.UtcNow + new TimeSpan(0, 0, 0, 0, 1000);

                //    while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
                //    {
                //        Yield();

                //        if (DateTime.UtcNow >= endtime)
                //        {
                //            return;
                //        }
                //    }
                //    GTA.UI.Screen.ShowSubtitle("Playing...");

                //    Function.Call(Hash.PLAY_ENTITY_ANIM,
                //        veh.Handle,
                //        animName,
                //        animDict, 1f, false, false); //, false, 255, 0x4000);
                //}
            }

            if (e.KeyCode == Keys.L)
            {
                var config = TrainInfo.Load("RogersSierra3");

                _ = (SteamTrain)TrainFactory.CreateTrain(config, Game.Player.Character.Position, true);
            }

            if (e.KeyCode == Keys.K)
            {
                ATSPool.Trains.DisposeAllAndClear();
            }
        }

        private void Update(object sender, EventArgs e)
        {
            //AnimTest();

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
                                continue;

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

        //float curr = 0f;
        //private void AnimTest()
        //{
        //    //var veh = Game.Player.Character.CurrentVehicle;
        //    //if (veh != null)
        //    //{
        //    //    string animDict = "anim@veh@sierra";
        //    //    string animName = "front_wheels_move";

        //    //    float totalTime = Function.Call<float>(Hash.GET_ENTITY_ANIM_TOTAL_TIME, veh, animDict, animName);
        //    //    Function.Call(Hash.SET_ENTITY_ANIM_CURRENT_TIME, veh, animDict, animName, curr);

        //    //    curr += 0.1f * Game.LastFrameTime;

        //    //    if (curr >= 0.98f)
        //    //        curr = 0;

        //    //    GTA.UI.Screen.ShowSubtitle($"Current: {curr:0.00}");
        //    //}


        //    //    // Remove dirt because it's not supported by train model
        //    //    LocomotiveCarriage.VisibleVehicle.DirtLevel = 0;
        //    //    TenderCarriage.VisibleVehicle.DirtLevel = 0;

        //    //    // May be damaged when spawning, we don't need it anyway
        //    //    LocomotiveCarriage.VisibleVehicle.PetrolTankHealth = 1000;
        //    //    TenderCarriage.VisibleVehicle.PetrolTankHealth = 1000;
        //}
    }
}
