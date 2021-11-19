using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Extensions;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using RageAudio;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// Fmod audio player of this script.
        /// </summary>
        public readonly AudioPlayer AudioPlayer;

        /// <summary>
        /// Constructs new instance of <see cref="Main"/>.
        /// </summary>
        public Main()
        {
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);
            File.AppendAllText($"./ScriptHookVDotNet.log", $"AdvancedTrainSystem - {Version} ({buildDate})" + Environment.NewLine);

            Tick += Update;
            KeyDown += OnKeyDown;
            Aborted += MainAborted;
        }

        /// <summary>
        /// Aborts 
        /// </summary>
        private void MainAborted(object sender, EventArgs e)
        {
            // Dispose only train components and invalidate handles
            // but keep vehicles, it will help to "hook" train after reloading
            ATSPool.Trains.DisposeComponents();
        }

        /// <summary>
        /// Handles debugging hotkeys.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Y)
            {
                // Debugging code...

                var veh = Game.Player.Character.CurrentVehicle;
                if (veh != null)
                {
                    GTA.UI.Screen.ShowSubtitle("Requesting...");

                    string animDict = "anim@veh@sierra";
                    string animName = "front_wheels_move";

                    Function.Call(Hash.REQUEST_ANIM_DICT, animDict);

                    var endtime = DateTime.UtcNow + new TimeSpan(0, 0, 0, 0, 1000);
                    
                    while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
                    {
                        Yield();

                        if (DateTime.UtcNow >= endtime)
                        {
                            return;
                        }
                    }
                    GTA.UI.Screen.ShowSubtitle("Playing...");

                    Function.Call(Hash.PLAY_ENTITY_ANIM,
                        veh.Handle,
                        animName,
                        animDict, 1f, false, false); //, false, 255, 0x4000);
                }
            }
        }

        float curr = 0f;
        /// <summary>
        /// Main loop function of the script.
        /// </summary>
        private void Update(object sender, EventArgs e)
        {
            //var veh = Game.Player.Character.CurrentVehicle;
            //if (veh != null)
            //{
            //    string animDict = "anim@veh@sierra";
            //    string animName = "front_wheels_move";

            //    float totalTime = Function.Call<float>(Hash.GET_ENTITY_ANIM_TOTAL_TIME, veh, animDict, animName);
            //    Function.Call(Hash.SET_ENTITY_ANIM_CURRENT_TIME, veh, animDict, animName, curr);

            //    curr += 0.1f * Game.LastFrameTime;

            //    if (curr >= 0.98f)
            //        curr = 0;

            //    GTA.UI.Screen.ShowSubtitle($"Current: {curr:0.00}");
            //}


            //    // Remove dirt because it's not supported by train model
            //    LocomotiveCarriage.VisibleVehicle.DirtLevel = 0;
            //    TenderCarriage.VisibleVehicle.DirtLevel = 0;

            //    // May be damaged when spawning, we don't need it anyway
            //    LocomotiveCarriage.VisibleVehicle.PetrolTankHealth = 1000;
            //    TenderCarriage.VisibleVehicle.PetrolTankHealth = 1000;

            if (_firstTick)
            {
                Constants.RegisterDecorators();

                // Hook trains from previous session

                var trains = World.GetAllVehicles();
                for (int i = 0; i < trains.Length; i++)
                {
                    var train = trains[i];
                    
                    // Make sure to pass only train head (locomotive hidden model)
                    // cuz otherwise it will be respawned for each carriage... we don't need that
                    if (train.IsAtsHead())
                    {
                        Train.Respawn(train);
                    }
                }

                _firstTick = false;
            }

            FusionUtils.RandomTrains = false;
        }
    }
}
