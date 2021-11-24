using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Components.Enums;
using AdvancedTrainSystem.Railroad.SharedComponents.Abstract;
using AdvancedTrainSystem.Railroad.SharedComponents.Interfaces;
using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.SharedComponents
{
    /// <summary>
    /// Defines that train have lights.
    /// </summary>
    /// <remarks>
    /// Must be used with <see cref="IHasElectricity"/>
    /// </remarks>
    public class LightComponent : Component
    {
        /// <summary>
        /// Current state of train lights.
        /// </summary>
        /// <remarks>
        /// Even if <see cref="LightState.HighBeam"/> or <see cref="LightState.LowBeam"/> is set,
        /// without working generator light won't work.
        /// </remarks>
        public LightState LightState
        {
            get => (LightState) train.Decorator.GetInt(Constants.TrainLightState);
            set
            {
                train.Decorator.SetInt(Constants.TrainLightState, (int) value);

                UpdateLight();
            }
        }

        private readonly Train train;
        private GeneratorComponent generator;

        public LightComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            generator = Components.GetComponent<GeneratorComponent>();

            // If we don't do this, all lights will appear dim
            // because lights are coming from visible model
            // but player is in invisible model
            // and gta makes lights dim for all traffic cars
            for (int i = 0; i < train.Carriages.Count; i++)
            {
                train.Carriages[i].Vehicle.SetPlayerLights(true);
            }

            UpdateLight();
        }

        /// <summary>
        /// Iterates through <see cref="Core.Components.Enums.LightState"/> switching modes of boiler light.
        /// </summary>
        public void SwitchHeadlight()
        {
            LightState = LightState.Next();
        }

        public override void Update()
        {
            if (Game.IsControlJustPressed(Control.VehicleHeadlight))
                SwitchHeadlight();
        }

        /// <summary>
        /// Checks if generator is working and depending on that 
        /// settings either <see cref="LightState"/> 
        /// or if generator isn't working - <see cref="LightState.Disabled"/>.
        /// </summary>
        private void UpdateLight()
        {
            bool lightState = false;
            bool highBeamState = false;

            if (generator.Output > 0f)
            {
                switch (LightState)
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
            for (int i = 0; i < train.Carriages.Count; i++)
            {
                Vehicle carriage = train.Carriages[i].Vehicle;

                carriage.AreLightsOn = lightState;
                carriage.AreHighBeamsOn = highBeamState;

                // Turn on/off engine cuz gta without engine
                // running there's no light
                carriage.IsEngineRunning = generator.Output > 0f;

                carriage.SetLightsBrightness(generator.Output);
            }
        }
    }
}
