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
        private readonly float _power;
        private readonly int _burnTime;

        private static readonly Random _rand = new Random();

        private int burnStartTime = -1;
        private int burnUntilTime = -1;

        /// <summary>
        /// Gets a value indicating if fuel is done.
        /// </summary>
        public bool Burned { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating how much heat fuel produces.
        /// </summary>
        public float Power => _power;

        /// <summary>
        /// Gets a value indicating how much time fuel will burn.
        /// </summary>
        public int BurnTime => _burnTime;

        /// <summary>
        /// Creates a new instance of <see cref="TrainFuel"/> with defined power.
        /// </summary>
        /// <param name="power">A multiplier, which defines how many heat it makes while burning.</param>
        /// <param name="burnTime">Defines how much time fuel will burn.</param>
        public TrainFuel(float power, int burnTime)
        {
            _power = power;
            _burnTime = burnTime;

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
            if(Game.GameTime < burnUntilTime)
            {
                // When fuel burns it's most efficient on beginning
                // and least efficient when it's almost burned
                float burnTimeLeft = (float) burnUntilTime - Game.GameTime;
                float powerMultiplier = burnTimeLeft.Remap(0f, BurnTime, 0f, 1f);

                return (float)_rand.NextDouble(0.05f, 0.1f) * Power * powerMultiplier * Game.LastFrameTime;
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
        /// Creates a new instance of <see cref="Coal"/>.
        /// <para>
        /// Coal burn with power of 1 for 10 minutes.
        /// </para>
        /// </summary>
        public Coal() : base(1f, 10 * 60 * 1000)
        {

        }
    }

    public class PrestoLog : TrainFuel
    {
        public PrestoLog() : base(100f, 15000)
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

        private readonly List<TrainFuel> Fuel = new List<TrainFuel>();

        /// <summary>
        /// Maximum pressure of the boiler in PSI.
        /// </summary>
        private const float _maxPsiPressure = 300;
        private const int _maxCoal = 25;
        private float _heatGain = 0;

        private readonly Train train;

        private ControlsComponent _controls;
        private SafetyValveComponent _safetyValve;

        /// <summary>
        /// Creates a new instance of <see cref="BoilerComponent"/>.
        /// </summary>
        /// <param name="components"></param>
        public BoilerComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
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

            // Make sure pressure doesn't go outside bounds
            //PressurePSI = MathExtensions.Clamp(PressurePSI, 0f, _maxPsiPressure);

            //GTA.UI.Screen.ShowSubtitle($"Coal: {Fuel.Count()} Boiler Pressure: {PressurePSI:0.00} {Pressure:0.00}");

            if (Game.IsControlJustPressed(Control.ThrowGrenade) && train.Driver == GPlayer)
            {
                AddCoal();
                GTA.UI.Screen.ShowHelpText($"Added coal. Total coal: {Fuel.Count}", 1500);
            }
        }

        /// <summary>
        /// Removes burned coal from firebox.
        /// </summary>
        private void CleanupFirebox()
        {
            Fuel.RemoveAll(coal => coal.Burned);
        }

        /// <summary>
        /// Adds a coal in firebox.
        /// </summary>
        /// <remarks>
        /// Burning coal heats the water to produce steam. 
        /// <para>
        /// Adding more coal increases boiler pressure.
        /// </para>
        /// </remarks>
        public void AddCoal()
        {
            if (Fuel.Count >= _maxCoal)
                return;

            Fuel.Add(new Coal());
        }

        /// <summary>
        /// Calculates pressure gain from burning coal.
        /// </summary>
        private float GetSteamGain()
        {
            _heatGain = Fuel.Sum(coal => coal.GetHeat());

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
