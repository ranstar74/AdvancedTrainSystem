using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    public class Chimney : Component
    {
        /// <summary>
        /// Gets a normalized value indicating how much theres air in boiler.
        /// </summary>
        public float AirInBoiler { get; private set; } = 1f;

        private Boiler _boiler;
        public Chimney(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            _boiler = Components.GetComponent<Boiler>();
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
