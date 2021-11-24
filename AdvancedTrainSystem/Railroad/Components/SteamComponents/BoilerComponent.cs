using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
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
        public float Pressure =>
            MathExtensions.Clamp(PressurePSI.Remap(0f, maxPsiPressure, 0f, 1f), 0f, 1f);

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
        /// Gets a normalized value indicating how much theres coal in the firebox.
        /// </summary>
        /// <remarks>
        /// Burning coal heats the water to produce steam. 
        /// <para>
        /// Adding more coal increases boiler pressure.
        /// </para>
        /// </remarks>
        public float Coal { get; private set; }

        /// <summary>
        /// Maximum pressure of the boiler in PSI.
        /// </summary>
        private const float maxPsiPressure = 300;

        private ControlsComponent controls;
        private SafetyValveComponent safetyValve;
        /// <summary>
        /// Creates a new instance of <see cref="BoilerComponent"/>.
        /// </summary>
        /// <param name="components"></param>
        public BoilerComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            controls = Components.GetComponent<ControlsComponent>();
            safetyValve = Components.GetComponent<SafetyValveComponent>();

            PressurePSI = 300f;
        }

        public override void Update()
        {
            float gain = GetSteamGain();
            float consumption = GetSteamConsumption();

            PressurePSI += gain;
            PressurePSI -= consumption;

            // Make sure pressure doesn't go outside bounds
            PressurePSI = MathExtensions.Clamp(PressurePSI, 0f, maxPsiPressure);

            GTA.UI.Screen.ShowSubtitle($"Boiler Pressure: {PressurePSI:0.00} {Pressure:0.00}");
        }

        /// <summary>
        /// Calculates pressure gain from burning coal.
        /// </summary>
        private float GetSteamGain()
        {
            return 1f * Game.LastFrameTime;
        }

        /// <summary>
        /// Calculates pressure consumption by safety valves, drain cocks,
        /// dynamo generator and other components this frame.
        /// </summary>
        private float GetSteamConsumption()
        {
            float throttle = controls.Throttle * 2 * Game.LastFrameTime;
            float safValve = safetyValve.Valve * 8 * Game.LastFrameTime;
            float cocks = controls.DrainCocks * 4 * Game.LastFrameTime;

            return throttle + safValve + cocks;
        }
    }
}
