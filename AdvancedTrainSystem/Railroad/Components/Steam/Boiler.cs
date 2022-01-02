using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Components;
using AdvancedTrainSystem.Railroad.Components.Common;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.UI;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    /// <summary>Simulates steam train boiler behaviour.</summary>
    public class Boiler : TrainComponent
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
        public float HeatGainThisFrame { get; private set; }

        /// <summary>Gets a value indicating fuel capacity of firebox.</summary>
        public int FireboxCapacity { get; } = 25;

        /// <summary>Gets a normalized value indicating amount of water in cylinders,
        /// if this value raises to high it could cause piston hydrolock.</summary>
        public float WaterInCylinders { get; private set; } = 0.6f;

        /// <summary>Maximum pressure of the boiler in PSI.</summary>
        private const float _maxPsiPressure = 300;

        private readonly List<TrainFuel> _fuel = new List<TrainFuel>();

        private SteamControls _controls;
        private SafetyValve _safetyValve;
        private Hydrobrake _hydrobrake;

        public Boiler(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            _controls = Components.GetComponent<SteamControls>();
            _safetyValve = Components.GetComponent<SafetyValve>();
            _hydrobrake = Components.GetComponent<Hydrobrake>();

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

            //if(Train.Driver == GPlayer)
            //{
            //    Screen.ShowHelpText(
            //        $"Capacity Left: {FuelCapacityLeft():0.0}\n" + 
            //        $"Boiler Pressure: {PressurePSI:0}\n" + 
            //        $"Water in Cylinders: {WaterInCylinders:0.0}", 1, false, false);
            //}

            if (Game.IsControlJustPressed(Control.ThrowGrenade) && Train.Driver == GPlayer)
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

            WaterInCylinders = WaterInCylinders.Clamp(0f, 1f);
            if(Physx.Speed < 1f)
            {
                WaterInCylinders += Game.LastFrameTime / 150;
            }

            WaterInCylinders -= (_controls.DrainCocks * Pressure > 0.1f ? 1f : 0f) * Game.LastFrameTime / 5;

            if (WaterInCylinders > 0.5f)
            {
                if(!_hydrobrake.IsHydrolocked && Physx.Speed > 6f)
                {
                    // Raise chances with higher amount of water, eventually it will be 100%
                    _hydrobrake.IsHydrolocked = FusionUtils.Random.NextDouble() * 100 < WaterInCylinders;
                }
            }
            else
            {
                _hydrobrake.IsHydrolocked = false;
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
            return Math.Max(FireboxCapacity - _fuel.Sum(x => x.CurrentSize), 0);
        }

        /// <summary>Calculates pressure gain from burning coal.</summary>
        private float GetSteamGain()
        {
            HeatGainThisFrame = _fuel.Sum(coal => coal.GetHeat());

            return HeatGainThisFrame;
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
