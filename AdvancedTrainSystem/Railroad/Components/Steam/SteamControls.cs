using AdvancedTrainSystem.Core.Components;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    /// <summary>
    /// Defines interactive controls inside train.
    /// </summary>
    public sealed class SteamControls : Controls
    {
        /// <summary>Gets or sets a normalized value indicating how much throttle is opened.</summary>
        public float Throttle { get; set; }

        /// <summary>Gets or sets a normalized value indicating how much drain cocks are opened.</summary>
        public float DrainCocks { get; set; }

        /// <summary>Gets or sets a normalized value indicating gear lever position.</summary>
        public float Gear { get; set; }

        /// <summary>Gets or sets a normalized value indicating airbrake lever position.</summary>
        public float AirBrake { get; set; } = 1f;

        public SteamControls(ComponentCollection components) : base(components)
        {

        }

        protected override void ResolveBehaviour(string behaviour, float value, bool resolved)
        {
            switch(behaviour)
            {
                case "Throttle":
                    Throttle = value;
                    resolved = true;
                    break;
                case "Cocks":
                    DrainCocks = value;
                    resolved = true;
                    break;
                case "Gear":
                    Gear = value;
                    resolved = true;
                    break;
                case "AirBrake":
                    AirBrake = value;
                    resolved = true;
                    break;
            }

            base.ResolveBehaviour(behaviour, value, resolved);
        }
    }
}
