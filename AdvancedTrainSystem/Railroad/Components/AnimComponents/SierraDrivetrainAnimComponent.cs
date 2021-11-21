using AdvancedTrainSystem.Core.Components.Abstract.AnimComponents;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Railroad.Components.AnimComponents
{
    public class SierraDrivetrainAnimComponent : AnimComponent
    {
        private SierraWheelAnimComponent sierraWheel;

        private AnimateProp CouplingRod;
        private AnimateProp ConnectingRod;
        private AnimateProp Piston;

        private AnimateProp CombinationLever;
        private AnimateProp RadiusRod;
        private AnimateProp ValveRod;

        private float _distanceToRod;

        private float _connectingRodLength;
        private float _radiusRodLength;

        private float _pistonRelativePosY;
        private float _pistonRelativePosZ;

        private float _valveRelativePosZ;

        public SierraDrivetrainAnimComponent(ComponentCollection components) : base(components)
        {
            CouplingRod = new AnimateProp(ModelHandler.SierraCouplingRod, train, "dwheel_1");
            ConnectingRod = new AnimateProp(ModelHandler.SierraConnectingRod, train, "dwheel_1");
            Piston = new AnimateProp(ModelHandler.SierraPiston, train, "piston");

            CombinationLever = new AnimateProp(ModelHandler.SierraCombinationLever, train, "combination_lever");
            CombinationLever.SpawnProp();

            RadiusRod = new AnimateProp(ModelHandler.SierraRadiusRod, CombinationLever, "radius_rod_mounting", Vector3.Zero, Vector3.Zero, false);
            RadiusRod.UseFixedRot = false;
            RadiusRod.SpawnProp();

            ValveRod = new AnimateProp(ModelHandler.SierraValveRod, RadiusRod, "radius_rod_end", Vector3.Zero, Vector3.Zero, false);
            ValveRod.UseFixedRot = false;

            Vehicle trainVehicle = train;

            // Calculate distance from mounting point of coupling rod to center of wheel
            var rodPos = trainVehicle.GetOffsetPosition(trainVehicle.Bones["rod"].Position);
            var wheelpos = trainVehicle.GetOffsetPosition(trainVehicle.Bones["dwheel_1"].Position);
            _distanceToRod = Vector3.Distance(rodPos, wheelpos) - 0.045f;

            rodPos = trainVehicle.GetOffsetPosition(RadiusRod.Prop.Position);
            wheelpos = trainVehicle.GetOffsetPosition(trainVehicle.Bones["valve_rod"].Position);
            _radiusRodLength = Vector3.Distance(rodPos, wheelpos);

            _connectingRodLength = ModelHandler.SierraConnectingRod.Model.GetSize().width - 0.375f;
            _pistonRelativePosY = trainVehicle.Bones["piston"].RelativePosition.Y;
            _pistonRelativePosZ = trainVehicle.Bones["piston"].RelativePosition.Z;

            _valveRelativePosZ = trainVehicle.Bones["valve_rod"].RelativePosition.Z;

            CouplingRod.SpawnProp();
            ConnectingRod.SpawnProp();
            Piston.SpawnProp();
            CombinationLever.SpawnProp();
            RadiusRod.SpawnProp();
            ValveRod.SpawnProp();
        }

        public override void Start()
        {
            base.Start();

            sierraWheel = Components.GetComponent<SierraWheelAnimComponent>();
        }

        public override void Update()
        {
            if (derail.IsDerailed)
                return;
            Vehicle trainVehicle = train;

            float angleRad = sierraWheel.DrivingWheelAngle.ToRad();
            float angleCos = (float)Math.Cos(angleRad);
            float angleSin = (float)Math.Sin(angleRad);

            float dY = angleCos * _distanceToRod;
            float dZ = angleSin * _distanceToRod;

            CouplingRod.SetOffset(Coordinate.Y, dY);
            CouplingRod.SetOffset(Coordinate.Z, dZ);

            ConnectingRod.SetOffset(Coordinate.Y, dY);
            ConnectingRod.SetOffset(Coordinate.Z, dZ);

            float dAngle = 90 - MathExtensions.ToDeg(
                (float)MathExtensions.ArcCos(
                    (_pistonRelativePosZ - ConnectingRod.RelativePosition.Z) / _connectingRodLength));

            ConnectingRod.SetRotation(Coordinate.X, dAngle, true);

            Piston.SetOffset(
                Coordinate.Y, _connectingRodLength * (float)Math.Cos(
                    MathExtensions.ToRad(dAngle)) - (_pistonRelativePosY - ConnectingRod.RelativePosition.Y), true);

            dAngle = sierraWheel.DrivingWheelAngle;
            if (dAngle < 180)
                dAngle = dAngle.Remap(0, 180, 0, -12);
            else
                dAngle = dAngle.Remap(180, 360, -12, 0);

            CombinationLever.SetRotation(Coordinate.X, dAngle);

            dAngle = 90 - MathExtensions.ToDeg(
                (float)MathExtensions.ArcCos(
                    (_valveRelativePosZ - Math.Abs(trainVehicle.GetPositionOffset(RadiusRod.Position).Z)) / _radiusRodLength));

            RadiusRod.SetRotation(train.Rotation.GetSingleOffset(Coordinate.X, dAngle));

            ValveRod.SetRotation(train.Rotation);
        }

        public override void Dispose()
        {
            CouplingRod.Dispose();
            ConnectingRod.Dispose();
            Piston.Dispose();
            CombinationLever.Dispose();
            RadiusRod.Dispose();
            ValveRod.Dispose();
        }
    }
}
