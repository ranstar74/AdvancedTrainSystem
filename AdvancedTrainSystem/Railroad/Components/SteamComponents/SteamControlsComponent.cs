using FusionLibrary.Extensions;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>
    /// Defines base steam trains controls
    /// <list type="bullet">
    ///     <item>Throttle Lever</item>
    ///     <item>Gear Lever (also known as Reverse / Gear Lever)</item> 
    ///     <item>Steam Brake</item>
    ///     <item>Air Brake</item>
    /// </list>
    /// </summary>
    public class SteamTrainControlComponent : Component
    {
        // Deprecated

        /// <summary>
        /// Position of throttle lever.
        /// </summary>
        /// <remarks>
        /// 0 - throttle closed. 1 - fully opened.
        /// </remarks>
        public float ThrottleLeverPosition
        {
            get => _throttleLeverState;
            set
            {
                value = value.Clamp(0, 1);

                if (value == _throttleLeverState)
                    return;

                _throttleLeverState = value;
            }
        }

        /// <summary>
        /// Gets or sets position of gear lever. 1 - Forward. 0 - Neutral. -1 - Reverse.
        /// </summary>
        public float GearLeverPosition
        {
            get => _gearLeverState;
            set
            {
                value = value.Clamp(-1, 1);

                if (value == _gearLeverState)
                    return;

                _gearLeverState = value;
            }
        }

        /// <summary>
        /// Current position of full brake. 0 - Wheels moving. 1 - Wheels blocked.
        /// </summary>
        public int FullBrakeLeverState
        {
            get => _fullBrakeLeverState;
            set
            {
                value = value.Clamp(0, 1);

                if (value == _fullBrakeLeverState)
                    return;

                _fullBrakeLeverState = value;
            }
        }

        /// <summary>
        /// Current position of air brake. 0 - no brake. 1 - full brake.
        /// </summary>
        public float AirBrakeLeverState
        {
            get => _airbrakeLeverState;
            set
            {
                value = value.Clamp(0, 1);

                if (value == _airbrakeLeverState)
                    return;

                _airbrakeLeverState = value;
            }
        }

        private float _gearLeverState;
        private float _airbrakeLeverState;
        private float _throttleLeverState;
        private int _fullBrakeLeverState;

        public SteamTrainControlComponent(ComponentCollection components) : base(components)
        {

        }
    }
}
