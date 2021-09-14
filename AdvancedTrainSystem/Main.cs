using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Train;
using GTA;
using System;

namespace AdvancedTrainSystem
{
    internal class Main : Script
    {
        private bool _firstTick = true;

        internal Main()
        {
            Tick += OnTick;
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
