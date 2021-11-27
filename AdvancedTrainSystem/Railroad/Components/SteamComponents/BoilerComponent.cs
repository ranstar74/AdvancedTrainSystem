using AdvancedTrainSystem.Core;
using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    /// <summary>
    /// Defines a train fuel.
    /// </summary>
    public abstract class TrainFuel
    {
        /// <summary>
        /// Gets a value indicating how much heat fuel produces while burning.
        /// </summary>
        public abstract float Power { get; }

        /// <summary>
        /// Gets a value indicating how much time fuel will burn.
        /// </summary>
        public abstract int BurnTime { get; }

        /// <summary>
        /// Gets a value indicating abstract size of fuel. 
        /// <para>
        /// Used to calculate how many of fuel can be in firebox.
        /// </para>
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// Gets a value indicating if fuel is done.
        /// </summary>
        public bool Burned { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating how much size left from original size after burning.
        /// </summary>
        public float CurrentSize => Size * Health;

        /// <summary>
        /// Gets a normalized value indicating health of fuel.
        /// More time fuel burns, less health fuel have.
        /// </summary>
        public float Health => _health;

        private float _health;

        private static readonly Random _rand = new Random();

        private int burnStartTime = -1;
        private int burnUntilTime = -1;

        /// <summary>
        /// Creates a new instance of <see cref="TrainFuel"/> with defined power.
        /// </summary>
        public TrainFuel()
        {
            Ignite();
        }

        /// <summary>
        /// Ignites fuel.
        /// </summary>
        private void Ignite()
        {
            if (burnStartTime != -1)
                return;

            burnStartTime = Game.GameTime;
            burnUntilTime = burnStartTime + BurnTime;
        }

        /// <summary>
        /// Gets heat from burning fuel.
        /// </summary>
        public float GetHeat()
        {
            if (Game.GameTime < burnUntilTime)
            {
                // When fuel burns it's most efficient on beginning
                // and least efficient when it's almost burned
                float burnTimeLeft = (float)burnUntilTime - Game.GameTime;

                _health = burnTimeLeft.Remap(0f, BurnTime, 0f, 1f);

                return (float)_rand.NextDouble(0.05f, 0.1f) * Power * _health * Game.LastFrameTime;
            }
            Burned = true;

            return 0f;
        }
    }

    /// <summary>
    /// Defines a burnable coal.
    /// </summary>
    public class Coal : TrainFuel
    {
        /// <summary>
        /// Gets a value indicating how much heat coal produces while burning.
        /// </summary>
        public override float Power => 1f;

        /// <summary>
        /// Gets a value indicating how much time coal will burn.
        /// </summary>
        public override int BurnTime => 10 * 60 * 1000;

        /// <summary>
        /// Gets a value indicating abstract size of coal.
        /// </summary>
        public override int Size => 1;

        /// <summary>
        /// Creates a new instance of <see cref="Coal"/> with size of 1.
        /// <para>
        /// Coal burn with power of 1 for 10 minutes.
        /// </para>
        /// </summary>
        public Coal()
        {

        }
    }

    /// <summary>
    /// Simulates steam train boiler behaviour.
    /// </summary>
    public class BoilerComponent : Component
    {
        /// <summary>
        /// Gets a value indicating boiler pressure in PSI.
        /// </summary>
        public float PressurePSI { get; private set; }

        /// <summary>
        /// Gets a normalized value indicating boiler pressure.
        /// </summary>
        /// <remarks>
        /// Pressure in the boiler drops as the steam is consumed 
        /// and also when water is injected into the boiler.
        /// </remarks>
        public float Pressure => PressurePSI.Remap(0f, _maxPsiPressure, 0f, 1f);

        /// <summary>
        /// Gets a normalized value indicating water level in boiler.
        /// <para>
        /// Needs to be keeped around 0.6f - 0.7f (60-70%)
        /// </para>
        /// </summary>
        public float Water { get; private set; }

        /// <summary>
        /// Gets a normalized value indicating how much water is going in to the boiler.
        /// </summary>
        /// <remarks>
        /// When set to more than zero, increases water level in the boiler.
        /// </remarks>
        public float WaterInjector { get; private set; }

        /// <summary>
        /// Amount of heat gained this frame from burning fuel.
        /// </summary>
        public float HeatGainThisFrame => _heatGain;

        /// <summary>
        /// Gets a value indicating fuel capacity of firebox.
        /// </summary>
        public int FireboxCapacity => _maxCapacity;

        /// <summary>
        /// Maximum pressure of the boiler in PSI.
        /// </summary>
        private const float _maxPsiPressure = 300;

        private readonly List<TrainFuel> _fuel = new List<TrainFuel>();

        private const int _maxCapacity = 25;
        private float _heatGain = 0;

        private readonly Train _train;

        private ControlsComponent _controls;
        private SafetyValveComponent _safetyValve;

        /// <summary>
        /// Creates a new instance of <see cref="BoilerComponent"/>.
        /// </summary>
        /// <param name="components"></param>
        public BoilerComponent(ComponentCollection components) : base(components)
        {
            _train = GetParent<Train>();
        }

        public override void Start()
        {
            _controls = Components.GetComponent<ControlsComponent>();
            _safetyValve = Components.GetComponent<SafetyValveComponent>();

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

                GTA.UI.Screen.ShowHelpText(message, 1500);
            }
        }

        /// <summary>
        /// Removes burned coal from firebox.
        /// </summary>
        private void CleanupFirebox()
        {
            _fuel.RemoveAll(coal => coal.Burned);
        }

        /// <summary>
        /// Adds a fuel in firebox.
        /// </summary>
        /// <remarks>
        /// Burning fuel heats the water to produce steam. 
        /// <para>
        /// Adding more fuel increases boiler pressure.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Calculates how much there's size left in firebox.
        /// </summary>
        public float FuelCapacityLeft()
        {
            return Math.Max(_maxCapacity - _fuel.Sum(x => x.CurrentSize), 0);
        }

        /// <summary>
        /// Calculates pressure gain from burning coal.
        /// </summary>
        private float GetSteamGain()
        {
            _heatGain = _fuel.Sum(coal => coal.GetHeat());

            return _heatGain;
        }

        /// <summary>
        /// Calculates pressure consumption by safety valves, drain cocks,
        /// dynamo generator and other components this frame.
        /// </summary>
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
