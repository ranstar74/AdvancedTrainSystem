using AdvancedTrainSystem.Core.Components.Abstract;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    public class AirbrakeComponent : BrakeComponent
    {
        public override float Intensity => intensity;

        private float intensity;

        public AirbrakeComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {

        }
    }
}
