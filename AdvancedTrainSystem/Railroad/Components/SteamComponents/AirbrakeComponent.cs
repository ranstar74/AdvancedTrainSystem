using AdvancedTrainSystem.Core.Components.Abstract;
using GTA;
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
        public override float Intensity => 1f;

        public AirbrakeComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            // Re-arrange 0 - 1 to 1 - 0
            float brakeForce = 1 - Force;
            brakeForce = physx.AbsoluteSpeed * brakeForce;

            //physx.ApplyForce(-brakeForce * Game.LastFrameTime);
        }
    }
}
