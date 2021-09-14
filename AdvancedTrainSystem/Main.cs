using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Train;
using GTA;
using System;

namespace AdvancedTrainSystem
{
    public class Main : Script
    {
        private bool _firstTick = true;

        public Main()
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
