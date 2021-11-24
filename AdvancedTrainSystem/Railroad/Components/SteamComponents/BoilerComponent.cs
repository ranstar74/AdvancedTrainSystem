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
        public float Pressure { get; private set; }

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

            // Todo make proper handling of presure
            // by adding "coal" as resource, which will be
            // transformed into pressure. No free energy!
            Pressure = 1f;

            PressurePSI = Pressure.Remap(0f, 1f, 0f, maxPsiPressure);
        }

        public override void Update()
        {
            //Pressure += 3f * Game.LastFrameTime;
            //SafetyValve = SafetyValve.Clamp(0, 1);

            //SafetyValve = Pressure.Remap(200, 260, 0, 1);

            //// Safety valve
            //if (Pressure > 260)
            //{
            //    _releaseTime = Game.GameTime + 1000;
            //}
            //else
            //{
            //    _releaseTime = 0;
            //}

            //var throttle = Parent.Components.SpeedComponent.Throttle;

            //Pressure -= 3.1f * throttle * Game.LastFrameTime;

            float gain = GetSteamGain();
            float consumption = GetSteamConsumption();

            PressurePSI += gain;
            PressurePSI -= consumption;

            // Make sure pressure doesn't go outside bounds
            PressurePSI = MathExtensions.Clamp(PressurePSI, 0f, maxPsiPressure);

            //GTA.UI.Screen.ShowSubtitle($"Boiler Pressure: {PressurePSI:0.00}");
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

            return throttle + safValve;
        }
    }
}
