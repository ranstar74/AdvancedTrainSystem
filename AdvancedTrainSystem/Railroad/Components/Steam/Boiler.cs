using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Railroad.Components.Common;
using FusionLibrary.Extensions;
using GTA;
using GTA.UI;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    /// <summary>Simulates steam train boiler behaviour.</summary>
    public class Boiler : Component
    {
        /// <summary>Gets a value indicating boiler pressure in PSI.</summary>
        public float PressurePSI { get; private set; }

        /// <summary>Gets a normalized value indicating boiler pressure.</summary>
        /// <remarks>Pressure in the boiler drops as the steam is consumed 
        /// and also when water is injected into the boiler.</remarks>
        public float Pressure => PressurePSI.Remap(0f, _maxPsiPressure, 0f, 1f);

        /// <summary>Gets a normalized value indicating water level in boiler.
        /// <para>Needs to be keeped around 0.6f - 0.7f (60-70%)</para></summary>
        public float Water { get; private set; }

        /// <summary>Gets a normalized value indicating how much water is going in to the boiler.</summary>
        /// <remarks>When set to more than zero, increases water level in the boiler.</remarks>
        public float WaterInjector { get; private set; }

        /// <summary>Amount of heat gained this frame from burning fuel.</summary>
        public float HeatGainThisFrame => _heatGain;

        /// <summary>Gets a value indicating fuel capacity of firebox.</summary>
        public int FireboxCapacity => _maxCapacity;

        /// <summary>Maximum pressure of the boiler in PSI.</summary>
        private const float _maxPsiPressure = 300;

        private readonly List<TrainFuel> _fuel = new List<TrainFuel>();

        private const int _maxCapacity = 25;
        private float _heatGain = 0;

        private readonly Train _train;

        private SteamControls _controls;
        private SafetyValve _safetyValve;

        public Boiler(ComponentCollection components) : base(components)
        {
            _train = GetParent<Train>();
        }

        public override void Start()
        {
            _controls = Components.GetComponent<SteamControls>();
            _safetyValve = Components.GetComponent<SafetyValve>();

            PressurePSI = 300f;
        }

        public override void Update()
        {
            CleanupFirebox();

            float gain = GetSteamGain();
            float consumption = GetSteamConsumption();

            PressurePSI += gain;
            PressurePSI -= consumption;

            PressurePSI = Math.Max(PressurePSI, 0);

            if(_train.Driver == GPlayer)
            {            
                GTA.UI.Screen.ShowHelpText(
                    $"Capacity Left: {FuelCapacityLeft():0.0}\n" + 
                    $"Boiler Pressure: {PressurePSI:0}", 1, false, false);
            }

            if (Game.IsControlJustPressed(Control.ThrowGrenade) && _train.Driver == GPlayer)
            {
                string message;
                if (AddFuel<Coal>())
                {
                    float leftPercent = 100 - (_fuel.Sum(x => x.CurrentSize) * 100 / FireboxCapacity);

                    message =
                        "Added coal.\n" +
                        $"Capacity Left: {leftPercent / 100:P1}";
                }
                else
                    message = $"Not enough free space in firebox to add more fuel.";

                Screen.ShowHelpText(message, 1500);
            }
        }

        /// <summary>Removes burned coal from firebox.</summary>
        private void CleanupFirebox()
        {
            _fuel.RemoveAll(coal => coal.Burned);
        }

        /// <summary>Adds a fuel in firebox.</summary>
        /// <remarks>Burning fuel heats the water to produce steam.</remarks>
        /// <returns>True if fuel was added, otherwise False.</returns>
        public bool AddFuel<T>() where T : TrainFuel
        {
            T newFuel = Activator.CreateInstance<T>();

            if (FuelCapacityLeft() > newFuel.Size)
            {
                _fuel.Add(newFuel);
                return true;
            }
            return false;
        }

        /// <summary>Calculates how much there's size left in firebox.</summary>
        public float FuelCapacityLeft()
        {
            return Math.Max(_maxCapacity - _fuel.Sum(x => x.CurrentSize), 0);
        }

        /// <summary>Calculates pressure gain from burning coal.</summary>
        private float GetSteamGain()
        {
            _heatGain = _fuel.Sum(coal => coal.GetHeat());

            return _heatGain;
        }

        /// <summary>Calculates pressure consumption by safety valves, drain cocks,
        /// dynamo generator and other components this frame.</summary>
        private float GetSteamConsumption()
        {
            float throttle = _controls.Throttle * 2 * Game.LastFrameTime;
            float gear = Math.Abs(_controls.Gear.Remap(0f, 1f, -1f, 1f));
            float safValve = _safetyValve.Valve * 8 * Game.LastFrameTime;
            float cocks = _controls.DrainCocks * 4 * Game.LastFrameTime;

            return (throttle * gear) + safValve + cocks;
        }
    }
}
