using FusionLibrary.Extensions;
using RageComponent;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles control components inside cab, such as throttle and brake levers.
    /// </summary>
    public class ControlComponent : Component<CustomTrain>
    {
        private float _throttleLeverState;
        /// <summary>
        /// Position of throttle lever. 0 - throttle closed. 1 - fully opened.
        /// </summary>
        public float ThrottleLeverState
        {
            get => _throttleLeverState;
            set
            {
                value = value.Clamp(0, 1);

                if (value == _throttleLeverState)
                    return;

                _throttleLeverState = value;
                Base.SpeedComponent.Throttle = _throttleLeverState;
            }
        }

        private float _gearLeverState;
        /// <summary>
        /// Current position of gear lever. 1 - Forward. 0 - Neutral. -1 - Reverse.
        /// </summary>
        public float GearLeverState
        {
            get => _gearLeverState;
            set
            {
                value = value.Clamp(0, 1);

                if (value == _gearLeverState)
                    return;

                _gearLeverState = value;
                Base.SpeedComponent.Gear = _gearLeverState;
            }
        }

        private int _fullBrakeLeverState;
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
                Base.BrakeComponent.FullBrakeForce = _fullBrakeLeverState;
            }
        }

        private float _airbrakeLeverState;
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
                Base.BrakeComponent.AirbrakeForce = _airbrakeLeverState;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {

        }
    }
}
