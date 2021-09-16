using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Train;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Windows.Forms;

namespace AdvancedTrainSystem
{
    public class Main : Script
    {
        private bool _firstTick = true;

        public Main()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.E)
            {

            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_firstTick)
            {
                Constants.RegisterDecorators();

                _firstTick = false;
            }

            CustomTrain.OnTick();
        }
    }
}
