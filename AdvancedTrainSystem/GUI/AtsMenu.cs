using AdvancedTrainSystem.Core.Info;
using FusionLibrary;
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
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "ats_textures", "ats_menu_banner");
            Subtitle = GetMenuTitle();
        }
    }

    internal class SpawnMenu : AtsMenu
    {
        public static readonly SpawnMenu Instance = new SpawnMenu();

        private SpawnMenu() : base("ats_spawnmenu")
        {

        }

        public override string GetItemDescription(string itemName)
        {
            throw new NotImplementedException();
        }

        public override string GetItemTitle(string itemName)
        {
            throw new NotImplementedException();
        }

        public override string GetItemValueDescription(string itemName, string valueName)
        {
            throw new NotImplementedException();
        }

        public override string GetItemValueTitle(string itemName, string valueName)
        {
            throw new NotImplementedException();
        }

        public override string GetMenuDescription()
        {
            throw new NotImplementedException();
        }

        public override string GetMenuTitle()
        {
            throw new NotImplementedException();
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            throw new NotImplementedException();
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            Items.Clear();
            
            foreach(TrainInfo trainInfo in TrainInfo.GetAllConfigs())
            {
                Items.Add(new NativeItem(trainInfo.Name, "TODO", "TODO"));
            }
        }

        public override void Tick()
        {
            throw new NotImplementedException();
        }
    }
}
