using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    public class ChimneyComponent : Component
    {
        /// <summary>
        /// Gets a normalized value indicating how much theres air in boiler.
        /// </summary>
        public float AirInBoiler { get; private set; } = 1f;

        private BoilerComponent _boiler;
        public ChimneyComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            _boiler = Components.GetComponent<BoilerComponent>();
        }

        public override void Update()
        {
            AirInBoiler = MathExtensions.Lerp(
                AirInBoiler, 
                1 - MathExtensions.Clamp(_boiler.HeatGainThisFrame * 100, 0f, 1f), 
                Game.LastFrameTime / 10);
        }
    }
}
