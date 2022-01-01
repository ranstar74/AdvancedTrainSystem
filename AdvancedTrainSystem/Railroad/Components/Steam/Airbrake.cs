using AdvancedTrainSystem.Railroad.Components.Common;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    /// <summary>Controls air brake on steam train.</summary>
    public class Airbrake : Brake
    {
        /// <summary>Gets a normalized value indicating air brake effecinty.</summary>
        public override float Efficiently { get; } = 0.2f;

        /// <summary>Force that is currently applied on brake.</summary>
        public override float Force => _controls.AirBrake;
            
        private SteamControls _controls;

        public Airbrake(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            _controls = Components.GetComponent<SteamControls>();
        }
    }
}
