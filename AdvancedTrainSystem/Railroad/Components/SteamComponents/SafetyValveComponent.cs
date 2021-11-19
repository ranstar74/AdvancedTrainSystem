using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    /// <summary>
    /// Safety valve keeps boiler pressure under control in simlar way to how rev limiter works.
    /// </summary>
    public class SafetyValveComponent : Component
    {
        /// <summary>
        /// Gets a normalized value indicating how much safety value is opened.
        /// </summary>
        public float Valve { get; private set; }

        /// <summary>
        /// How much time (ms) safety valve will be opened after reaching trigger point.
        /// </summary>
        private const float safetyValveTime = 1000;

        /// <summary>
        /// Boiler pressure at which safety valve triggers.
        /// </summary>
        private const float safetyValveTrigger = 260;

        private float releaseTime = 0;

        private BoilerComponent boiler;

        public SafetyValveComponent(ComponentCollection components) : base(components)
        {
            
        }

        public override void Start()
        {
            boiler = Components.GetComponent<BoilerComponent>();
        }

        public override void Update()
        {
            // Check if pressure is too high and if so trigger the safety valve
            if(boiler.PressurePSI > safetyValveTrigger && Valve <= 0.05f)
                releaseTime = Game.GameTime + safetyValveTime;

            // Open / close safety valve depending on if its triggered or not
            if (releaseTime > Game.GameTime)
            {
                OpenValve(Valve + Game.LastFrameTime);
            }
            else
            {
                OpenValve(Valve - Game.LastFrameTime);
            }
        }

        /// <summary>
        /// Opens safety valve on given value.
        /// </summary>
        /// <param name="value">Value to open safety valve on</param>
        private void OpenValve(float value)
        {
            Valve = value.Clamp(0f, 1f);
        }
    }
}
