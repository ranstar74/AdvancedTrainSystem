using AdvancedTrainSystem.Core;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    public class SteamGaugesComponent : Component
    {
        private readonly Train _train;
        private BoilerComponent _boiler;
        private AnimateProp _needleProp;

        private bool _wiggleCycle = false;
        private float _wiggle = 0f;

        public SteamGaugesComponent(ComponentCollection components) : base(components)
        {
            _train = GetParent<Train>();
        }

        public override void Start()
        {
            _boiler = Components.GetComponent<BoilerComponent>();

            CustomModel _pressureNeedleModel = new CustomModel("sierra_cab_pressure_needle");
            _pressureNeedleModel.Request();

            _needleProp = new AnimateProp(
                model: _pressureNeedleModel,
                entity: _train,
                boneName: "cab_pressure_needle");
            _needleProp.SpawnProp();
        }

        public override void Update()
        {
            float pressure = _boiler.Pressure;
            float pressureAngle = pressure.Remap(0f, 1f, 0f, 288f);
            float amplitude = Math.Max(pressure - 0.9f, 0) * 50;

            // Make "crazy" animation if pressure gone too far
            _needleProp.SetRotation(
                coordinate: FusionEnums.Coordinate.Y,
                value: pressureAngle + _wiggle * amplitude);

            // Wiggle speed
            float add = Game.LastFrameTime * 20;
            if (_wiggleCycle)
                _wiggle -= add;
            else
                _wiggle += add;

            if (!_wiggleCycle && _wiggle > 1f || _wiggleCycle && _wiggle < 0f)
                _wiggleCycle = !_wiggleCycle;
        }

        public override void Dispose()
        {
            _needleProp.Dispose();
        }
    }
}
