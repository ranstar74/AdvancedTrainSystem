using FusionLibrary;

namespace AdvancedTrainSystem.GUI
{
    internal class AtsText : CustomText
    {
        public static AtsText Instance { get; } = new AtsText();

        private AtsText() : base("ats")
        {

        }
    }
}
