using AdvancedTrainSystem.Core.Info;
using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace AdvancedTrainSystem.GUI
{
    internal abstract class AtsMenu : CustomNativeMenu
    {
        public AtsMenu(string name) : base(string.Empty)
        {
            InternalName = name;
            CustomText = AtsText.Instance;
            Subtitle = GetMenuDescription();
        }
    }

    internal class SpawnMenu : AtsMenu
    {
        public static readonly SpawnMenu Instance = new SpawnMenu();

        private readonly NativeItem _deleteAllItem;
        private readonly NativeItem _deleteOtherItem;

        private SpawnMenu() : base("Spawn")
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(100, 200), "ats_textures", "ats_menu_banner");

            _deleteAllItem = new NativeItem("Delete All", "");
            _deleteOtherItem = new NativeItem("Delete Other", "");
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            GTA.UI.Screen.ShowSubtitle("OK");
            if (sender.Tag is TrainInfo trainInfo)
            {
                TrainFactory.CreateTrain(trainInfo, Game.Player.Character.Position, true);
                return;
            }

            if(sender == _deleteAllItem)
            {
                ATSPool.Trains.DisposeAllAndClear();
                return;
            }

            if(sender == _deleteOtherItem)
            {
                throw new NotImplementedException();
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            Items.Clear();

            foreach(TrainInfo trainInfo in TrainInfo.GetAllConfigs())
            {
                var item = new NativeItem(trainInfo.Name, trainInfo.Description)
                {
                    Tag = trainInfo
                };

                Items.Add(item);
            }
            Items.Add(_deleteAllItem);
            Items.Add(_deleteOtherItem);

            SelectedIndex = 0;
        }

        public override void Tick()
        {

        }
    }
}
