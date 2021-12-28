﻿using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Components;
using AdvancedTrainSystem.Core.Components.Enums;
using AdvancedTrainSystem.Railroad.SharedComponents.Abstract;
using AdvancedTrainSystem.Railroad.SharedComponents.Interfaces;
using FusionLibrary;
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
            get => (LightState) _train.Decorator.GetInt(Constants.TrainLightState);
            set
            {
                _train.Decorator.SetInt(Constants.TrainLightState, (int) value);

                UpdateLight();
            }
        }

        private readonly Train _train;
        private NativeInput lightSwitchInput = new NativeInput(Control.VehicleHeadlight);
        private GeneratorComponent _generator;
        private DrivingComponent _drive;

        public LightComponent(ComponentCollection components) : base(components)
        {
            _train = GetParent<Train>();

            UpdateTime = 100;
        }

        public override void Start()
        {
            _generator = Components.GetComponent<GeneratorComponent>();
            _drive = Components.GetComponent<DrivingComponent>();

            // If we don't do this, all lights will appear dim
            // because lights are coming from visible model
            // but player is in invisible model
            // and gta makes lights dim for all traffic cars
            for (int i = 0; i < _train.Carriages.Count; i++)
            {
                _train.Carriages[i].Vehicle.SetPlayerLights(true);
            }

            lightSwitchInput.OnControlJustPressed += () =>
            {
                if (!_drive.IsControlledByPlayer)
                    return;

                SwitchHeadlight();
            };
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
            UpdateLight();
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

            if (_generator.Output > 0f)
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
            for (int i = 0; i < _train.Carriages.Count; i++)
            {
                Vehicle carriage = _train.Carriages[i].Vehicle;

                carriage.AreLightsOn = lightState;
                carriage.AreHighBeamsOn = highBeamState;

                // Turn on/off engine cuz gta without engine
                // running there's no light
                carriage.IsEngineRunning = _generator.Output > 0f;

                carriage.SetLightsBrightness(_generator.Output);
            }
        }
    }
}
