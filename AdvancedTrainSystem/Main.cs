﻿using AdvancedTrainSystem.Data;
using FusionLibrary;
using GTA;
using System;
using System.Windows.Forms;

namespace AdvancedTrainSystem
{
    /// <summary>
    /// Main class of ATS.
    /// </summary>
    public class Main : Script
    {
        private bool _firstTick = true;

        /// <summary>
        /// Constructs new instance of <see cref="Main"/>.
        /// </summary>
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
            FusionUtils.RandomTrains = false;

            //CustomTrain.OnTick();
        }
    }
}
