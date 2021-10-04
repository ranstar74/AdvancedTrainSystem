using AdvancedTrainSystem.Data;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.Native;
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
            //if(e.KeyCode == Keys.E)
            //{
            //    var shovel = new CustomModel("prop_ld_shovel");
            //    shovel.Request();

            //    var prop = World.CreateProp(shovel, Game.Player.Character.Position, true, true);

            //    //prop.AttachTo(Game.Player.Character, Game.Player.Character.Bones[Bone.PHRightHand].RelativePosition, Vector3.Zero);
            //    Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY,
            //        prop, Game.Player.Character,
            //        Game.Player.Character.Bones[Bone.PHRightHand].Index, 0f, 0f, 0f, 0f, 0f, 0f, false, false, false, false, 2, true);

            //    Game.Player.Character.Task.PlayAnimation("random@burial", "a_burial", 1, 5000, AnimationFlags.Loop);
            //    //Game.Player.Character.Position = new GTA.Math.Vector3(154.92f, 6841.12f, 19.14f);
            //}
            //if(e.KeyCode == Keys.Y)
            //{
            //    Game.Player.Character.Task.PlayAnimation("random@burial", "a_burial_stop");
            //}
            //if(e.KeyCode == Keys.H)
            //{
            //    var duration = Function.Call<float>(Hash.GET_ANIM_DURATION, "random@burial", "a_burial");
            //    Function.Call(Hash.SET_ENTITY_ANIM_CURRENT_TIME, Game.Player.Character, "random@burial", "a_burial", duration / 2);
            //}
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
