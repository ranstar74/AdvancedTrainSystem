using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Extensions;
using FusionLibrary.Extensions;
using GTA;
using RageComponent;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles all electric components such as lights that powered by dynamo generator.
    /// </summary>
    public class DynamoComponent : Component<CustomTrain>
    {
        /// <summary>
        /// Current state of boiler light.
        /// </summary>
        public LightState BoilerLightState { get; set; } = LightState.Disabled;

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
            BoilerLightState = (LightState) Entity.Decorator().GetInt(Constants.TrainLightState);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {
            // TODO: Add support for other carriages lights

            ((Vehicle)Entity).IsEngineRunning = IsDynamoWorking;
            ProcessBoilerLight();
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

            ((Vehicle)Entity).AreLightsOn = lightState;
            ((Vehicle)Entity).AreHighBeamsOn = highBeamState;

            Entity.Decorator().SetInt(Constants.TrainLightState, (int) BoilerLightState);
        }
    }
}
