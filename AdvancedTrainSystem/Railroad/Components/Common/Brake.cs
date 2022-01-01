using AdvancedTrainSystem.Core.Components;
using GTA;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Railroad.Components.Common
{
    /// <summary>Describes a train brakes.</summary>
    public abstract class Brake : TrainComponent
    {
        /// <summary>Gets a normalized value indicating brake effecinty.</summary>
        public abstract float Efficiently { get; }

        /// <summary>Gets a normalized value indicating how much brake is applied.</summary>
        public abstract float Force { get; }

        public Brake(ComponentCollection components) : base(components)
        {

        }

        public override void Update()
        {
            // Speed resistance force, when train drives slower it gets higher.
            // So train will stop faster on lower speed
            float resistanceForce = (float) Math.Log(Physx.AbsoluteSpeed / 5, 0.5) / 10;
            resistanceForce = Math.Max(1, resistanceForce);

            if(Physx.VisualSpeed == 0)
            {
                resistanceForce = 0;
            }

            if (Physx.Speed < 0)
            {
                resistanceForce *= -1;
            }

            float brakeForce = Force * resistanceForce * Efficiently * Game.LastFrameTime;
            Physx.ApplyResistanceForce(-brakeForce);
        }
    }
}
