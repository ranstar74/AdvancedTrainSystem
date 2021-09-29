using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Extensions;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles all electric components such as lights that powered by dynamo generator.
    /// </summary>
    public class DynamoComponent : Component<CustomTrain>
    {
        private LightState _boilerLightState = LightState.Disabled;
        /// <summary>
        /// Current state of boiler light.
        /// </summary>
        public LightState BoilerLightState
        {
            get => _boilerLightState;
            set
            {
                _boilerLightState = value;
                ProcessBoilerLight();
            }
        }

        private bool _prevIsDynamoWorking;
        /// <summary>
        /// Whether dynamo generator is currently on or not.
        /// </summary>
        public bool IsDynamoWorking => Base.BoilerComponent.Pressure > 160;

        /// <summary>
        /// Iterates through <see cref="LightState"/> switching modes of boiler light.
        /// </summary>
        public void SwitchHeadlight()
        {
            BoilerLightState = BoilerLightState.Next();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Start()
        {
            for (int i = 0; i < Base.Carriages.Count; i++)
            {
                Base.Carriages[i].VisibleVehicle.SetPlayerLights(true);
            }

            BoilerLightState = (LightState)Entity.Decorator().GetInt(Constants.TrainLightState);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {
            if(IsDynamoWorking != _prevIsDynamoWorking)
                ProcessBoilerLight();

            _prevIsDynamoWorking = IsDynamoWorking;
        }

        /// <summary>
        /// Will change vehicle light depending on <see cref="BoilerLightState"/>.
        /// </summary>
        private void ProcessBoilerLight()
        {
            bool lightState = false;
            bool highBeamState = false;

            if (IsDynamoWorking)
            {
                switch (BoilerLightState)
                {
                    case LightState.Disabled:
                        {
                            lightState = false;
                            highBeamState = false;
                            break;
                        }
                    case LightState.LowBeam:
                        {
                            lightState = true;
                            highBeamState = false;
                            break;
                        }
                    case LightState.HighBeam:
                        {
                            lightState = true;
                            highBeamState = true;
                            break;
                        }
                }
            }
            // TODO: Fix lights not working for tender
            for (int i = 0; i < Base.Carriages.Count; i++)
            {
                var carriage = Base.Carriages[i].VisibleVehicle;

                carriage.AreLightsOn = lightState;
                carriage.AreHighBeamsOn = highBeamState;
                carriage.IsEngineRunning = IsDynamoWorking;

                if (carriage != Base.TrainHeadVisible)
                    carriage.SetLightsBrightness(lightState ? 1 : 0);
            }

            Entity.Decorator().SetInt(Constants.TrainLightState, (int) BoilerLightState);
        }
    }
}
