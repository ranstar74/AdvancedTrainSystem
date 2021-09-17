using FusionLibrary.Extensions;
using RageComponent;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles train brakes.
    /// </summary>
    public class BrakeComponent : Component<CustomTrain>
    {
        private float _airbrakeForce;
        /// <summary>
        /// Current level of airbrake. 0 - no brake, 1 - full brake.
        /// </summary>
        public float AirbrakeForce
        {
            get => _airbrakeForce;
            set
            {
                _airbrakeForce = value.Clamp(0, 1);

                if (Base.CabComponent.AirBrakeLeverState != _airbrakeForce)
                    Base.CabComponent.AirBrakeLeverState = _airbrakeForce;
            }
        }

        private int _steamBrake;
        /// <summary>
        /// Full brake blocks any wheel movement.
        /// </summary>
        public int FullBrakeForce
        {
            get => _steamBrake;
            set
            {
                _steamBrake = value.Clamp(0, 1);

                if (Base.CabComponent.FullBrakeLeverState != _steamBrake)
                    Base.CabComponent.FullBrakeLeverState = _steamBrake;
            }
        }
    }
}
