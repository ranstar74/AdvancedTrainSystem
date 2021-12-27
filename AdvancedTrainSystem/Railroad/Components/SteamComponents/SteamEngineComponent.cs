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
        private const float _accelerationMultiplier = 0.55f;

        private ControlsComponent _controls;
        private BoilerComponent _boiler;
        private PhysxComponent _physx;

        public SteamEngineComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            _controls = Components.GetComponent<ControlsComponent>();
            _boiler = Components.GetComponent<BoilerComponent>();
            _physx = Components.GetComponent<PhysxComponent>();
        }

        public override void Update()
        {
            float throttle = _controls.Throttle;
            float gear = _controls.Gear.Remap(0f, 1f, -1f, 1f);

            float steamForce = throttle * _boiler.Pressure;

            // Get steam force direction
            float forceFactor = throttle <= 0.1f || Math.Abs(gear) <= 0.1f ? _physx.Speed : gear;
            if (forceFactor < 0)
                steamForce *= -1;

            steamForce *= _controls.Gear;

            float appliedForce = GetAppliedForceEffecienty(_physx.AbsoluteSpeed, Math.Abs(steamForce));

            _physx.DoWheelSlip = appliedForce < 0.4f;
            _physx.ApplyForce(steamForce * _accelerationMultiplier * Game.LastFrameTime);
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
