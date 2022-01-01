using AdvancedTrainSystem.Core.Data;
using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using System.Drawing;

namespace AdvancedTrainSystem.UI
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
            // SizeF - first param does nothing, second one sets height in pixels i guess
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(0, 220), "ats_textures", "ats_menu_banner");

            foreach (TrainData trainInfo in TrainData.GetAllConfigs())
            {
                var item = NewItem(trainInfo.LocalizationCode);
                item.Tag = trainInfo;
            }
            _deleteAllItem = NewItem("RemoveAll");
            _deleteOtherItem = NewItem("RemoveOther");
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender.Tag is TrainData trainInfo)
            {
                TrainFactory.CreateTrain(trainInfo, Game.Player.Character.Position, true);
            }
            else if(sender == _deleteAllItem)
            {
                TrainPool.Trains.DisposeAllAndClear();
            } 
            else if(sender == _deleteOtherItem)
            {
                throw new NotImplementedException();
            }

            Visible = false;
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

        }

        public override void Tick()
        {

        }
    }
}
