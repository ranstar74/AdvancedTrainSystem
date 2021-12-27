using FusionLibrary;

namespace AdvancedTrainSystem
{
    internal class AtsText : CustomText
    {
        public static AtsText Instance { get; } = new AtsText();

        private AtsText() : base("ats")
        {

        }
    }
}
