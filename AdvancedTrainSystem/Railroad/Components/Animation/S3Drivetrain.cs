using AdvancedTrainSystem.Core.Components;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Railroad.Components.AnimComponents
{
    public class S3Drivetrain : TrainComponent
    {
        private S3Wheel _sierraWheel;

        private readonly AnimateProp _couplingRod;
        private readonly AnimateProp _connectingRod;
        private readonly AnimateProp _piston;

        private readonly AnimateProp _combinationLever;
        private readonly AnimateProp _radiusRod;
        private readonly AnimateProp _valveRod;

        private readonly float _distanceToRod;

        private readonly float _connectingRodLength;
        private readonly float _radiusRodLength;

        private readonly float _pistonRelativePosY;
        private readonly float _pistonRelativePosZ;

        private readonly float _valveRelativePosZ;

        public S3Drivetrain(ComponentCollection components) : base(components)
        {
            _couplingRod = new AnimateProp(ModelHandler.SierraCouplingRod, Train, "dwheel_1");
            _connectingRod = new AnimateProp(ModelHandler.SierraConnectingRod, Train, "dwheel_1");
            _piston = new AnimateProp(ModelHandler.SierraPiston, Train, "piston");

            _combinationLever = new AnimateProp(ModelHandler.SierraCombinationLever, Train, "combination_lever");
            _combinationLever.SpawnProp();

            _radiusRod = new AnimateProp(ModelHandler.SierraRadiusRod, _combinationLever, "radius_rod_mounting", Vector3.Zero, Vector3.Zero, false);
            _radiusRod.UseFixedRot = false;
            _radiusRod.SpawnProp();

            _valveRod = new AnimateProp(ModelHandler.SierraValveRod, _radiusRod, "radius_rod_end", Vector3.Zero, Vector3.Zero, false);
            _valveRod.UseFixedRot = false;

            Vehicle trainVehicle = Train;

            // Calculate distance from mounting point of coupling rod to center of wheel
            var rodPos = trainVehicle.GetOffsetPosition(trainVehicle.Bones["rod"].Position);
            var wheelpos = trainVehicle.GetOffsetPosition(trainVehicle.Bones["dwheel_1"].Position);
            _distanceToRod = Vector3.Distance(rodPos, wheelpos) - 0.045f;

            rodPos = trainVehicle.GetOffsetPosition(_radiusRod.Prop.Position);
            wheelpos = trainVehicle.GetOffsetPosition(trainVehicle.Bones["valve_rod"].Position);
            _radiusRodLength = Vector3.Distance(rodPos, wheelpos);

            _connectingRodLength = ModelHandler.SierraConnectingRod.Model.GetSize().width - 0.375f;
            _pistonRelativePosY = trainVehicle.Bones["piston"].RelativePosition.Y;
            _pistonRelativePosZ = trainVehicle.Bones["piston"].RelativePosition.Z;

            _valveRelativePosZ = trainVehicle.Bones["valve_rod"].RelativePosition.Z;

            _couplingRod.SpawnProp();
            _connectingRod.SpawnProp();
            _piston.SpawnProp();
            _combinationLever.SpawnProp();
            _radiusRod.SpawnProp();
            _valveRod.SpawnProp();
        }

        public override void Start()
        {
            base.Start();

            _sierraWheel = Components.GetComponent<S3Wheel>();
        }

        public override void Update()
        {
            //if (derail.IsDerailed)
            //    return;

            Vehicle trainVehicle = Train;

            float angleRad = _sierraWheel.DrivingWheelAngle.ToRad();
            float angleCos = (float)Math.Cos(angleRad);
            float angleSin = (float)Math.Sin(angleRad);

            float dY = angleCos * _distanceToRod;
            float dZ = angleSin * _distanceToRod;

            _couplingRod.SetOffset(Coordinate.Y, dY);
            _couplingRod.SetOffset(Coordinate.Z, dZ);

            _connectingRod.SetOffset(Coordinate.Y, dY);
            _connectingRod.SetOffset(Coordinate.Z, dZ);

            float dAngle = 90 - MathExtensions.ToDeg(
                (float)MathExtensions.ArcCos(
                    (_pistonRelativePosZ - _connectingRod.RelativePosition.Z) / _connectingRodLength));

            _connectingRod.SetRotation(Coordinate.X, dAngle, true);

            _piston.SetOffset(
                Coordinate.Y, _connectingRodLength * (float)Math.Cos(
                    MathExtensions.ToRad(dAngle)) - (_pistonRelativePosY - _connectingRod.RelativePosition.Y), true);

            dAngle = _sierraWheel.DrivingWheelAngle;
            if (dAngle < 180)
                dAngle = dAngle.Remap(0, 180, 0, -12);
            else
                dAngle = dAngle.Remap(180, 360, -12, 0);

            _combinationLever.SetRotation(Coordinate.X, dAngle);

            dAngle = 90 - MathExtensions.ToDeg(
                (float)MathExtensions.ArcCos(
                    (_valveRelativePosZ - Math.Abs(trainVehicle.GetPositionOffset(_radiusRod.Position).Z)) / _radiusRodLength));

            _radiusRod.SetRotation(Train.Rotation.GetSingleOffset(Coordinate.X, dAngle));

            _valveRod.SetRotation(Train.Rotation);
        }

        public override void Dispose()
        {
            _couplingRod.Dispose();
            _connectingRod.Dispose();
            _piston.Dispose();
            _combinationLever.Dispose();
            _radiusRod.Dispose();
            _valveRod.Dispose();
        }
    }
}
