using AdvancedTrainSystem.Core.Components;
using AdvancedTrainSystem.Core.Components.Abstract;
using FusionLibrary.Extensions;
using GTA;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    /// <summary>
    /// Simulates steam engine behaviour.
    /// </summary>
    public class SteamEngineComponent : EngineComponent
    {
        private const float _accelerationMultiplier = 0.5f;

        private ControlsComponent controls;
        private BoilerComponent boiler;
        private PhysxComponent physx;

        public SteamEngineComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            controls = Components.GetComponent<ControlsComponent>();
            boiler = Components.GetComponent<BoilerComponent>();
            physx = Components.GetComponent<PhysxComponent>();
        }

        public override void Update()
        {
            float throttle = controls.Throttle;
            float gear = 1f;

            float steamForce = throttle * gear * boiler.Pressure;

            // Get steam force direction
            float forceFactor = throttle <= 0.1f || Math.Abs(gear) <= 0.1f ? physx.Speed : gear;
            if (forceFactor < 0)
                steamForce *= -1;

            float appliedForce = GetAppliedForceEffecienty(physx.Speed, steamForce);

            physx.DoWheelSlip = appliedForce < 0.4f;
            physx.ApplyForce(steamForce * _accelerationMultiplier * Game.LastFrameTime);
        }

        /// <summary>
        /// Calculates how much of torque will be applied to train longitudinal force.
        /// </summary>
        private static float GetAppliedForceEffecienty(float speed, float acceleration)
        {
            // A bit stupid way to do that but it works good enough?
            // It's hard to explain what i had in my head but just analyze these input/output values

            // Speed / Torque / Acceleration / Output
            // 1    204 000     8.5         0.24
            // 1    125 000     5.2         0.57
            // 1    47 000      1.9         0.91
            // 6    204 000     8.5         0.75
            // 6    125 000     5.2         0.92

            // So basically until 10m/s greater acceleration less output
            // This will simulate traction loose on high throttle

            float clamped = -Math.Min(speed - acceleration * 10, 0);
            return 1 - MathExtensions.Clamp(clamped / 10, 0, 1);
        }
    }
}
