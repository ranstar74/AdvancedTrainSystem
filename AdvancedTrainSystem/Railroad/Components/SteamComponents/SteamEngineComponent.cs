using AdvancedTrainSystem.Core.Components.Abstract;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    public class SteamEngineComponent : EngineComponent
    {
        private ControlsComponent controls;
        private BoilerComponent boiler;
        public SteamEngineComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            controls = Components.GetComponent<ControlsComponent>();
            boiler = Components.GetComponent<BoilerComponent>();
        }

        public override void Update()
        {
            float engineOutput = controls.Throttle * boiler.Pressure;

            GTA.UI.Screen.ShowSubtitle($"{engineOutput:0.0}");
        }
    }
}
