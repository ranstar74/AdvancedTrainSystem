using AdvancedTrainSystem.Data;
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
        private Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        private bool _firstTick = true;

        /// <summary>
        /// Constructs new instance of <see cref="Main"/>.
        /// </summary>
        public Main()
        {
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);
            System.IO.File.AppendAllText($"./ScriptHookVDotNet.log", $"AdvancedTrainSystem - {Version} ({buildDate})" + Environment.NewLine);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += MainAborted;
        }

        private void MainAborted(object sender, EventArgs e)
        {
            Debug.OnAbort();
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
                Debug.Start();

                _firstTick = false;
            }
            FusionUtils.RandomTrains = false;
        }
    }
}
