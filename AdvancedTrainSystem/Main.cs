using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Missions;
using AdvancedTrainSystem.Railroad;
using AdvancedTrainSystem.Railroad.Components.AnimComponents;
using AdvancedTrainSystem.UI;
using FusionLibrary;
using GTA;
using System;
using System.Linq;
using System.Windows.Forms;

namespace AdvancedTrainSystem
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class Main : Script
    {
        private bool _firstTick = true;

        public Main()
        {
            Tick += Update;
            KeyDown += OnKeyDown;
            Aborted += MainAborted;
        }

        private void MainAborted(object sender, EventArgs e)
        {
            AtsStoryMgr.Instance.Abort();

            TrainReloadMgr.SaveTrains();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Debugging code...

            if (e.KeyCode == Keys.L)
            {
                if (TrainPool.Trains.Count() != 0)
                {
                    SteamTrain train = (SteamTrain)TrainPool.Trains[0];
                    train.Components.Physx.Speed += 10;
                    train.Components.SteamControls.Throttle = 1f;
                    train.Components.SteamControls.Gear = 1f;
                }
            }

            if (e.KeyCode == Keys.Y)
            {
                SpawnMenu.Instance.Visible = true;
            }
        }

        private void Start()
        {
            TrainConstants.RegisterDecorators();
            ModelHandler.RequestAll();
            TrainReloadMgr.LoadTrains();
        }

        private void Update(object sender, EventArgs e)
        {
            AtsStoryMgr.Instance.Update();
            FusionUtils.RandomTrains = false;

            if (_firstTick)
            {
                Start();
                _firstTick = false;
            }
        }
    }
}
