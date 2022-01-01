using AdvancedTrainSystem.Core.Components;
using AdvancedTrainSystem.Core.Utils;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Railroad.Components.AnimComponents
{
    public class S3Wheel : TrainComponent
    {
        /// <summary>
        /// Gets angle of the driving wheel.
        /// </summary>
        public float DrivingWheelAngle => _driveWheels[0].SecondRotation.X;

        /// <summary>
        /// Length of the drive wheel.
        /// </summary>
        public float DriveWheelLength => _driveLength;

        private readonly AnimatePropsHandler _driveWheels = new AnimatePropsHandler();
        private readonly AnimatePropsHandler _frontWheels = new AnimatePropsHandler();
        private readonly AnimatePropsHandler _tenderWheels = new AnimatePropsHandler();

        private float _driveLength;
        private float _frontLength;
        private float _tenderLength;

        public S3Wheel(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            // Length of cylinder is diameter * pi

            _frontLength = (float)(ModelHandler.SierraFrontWheel.Model.GetSize().height * Math.PI);
            _driveLength = (float)(ModelHandler.SierraDrivingWheel.Model.GetSize().height * Math.PI);
            _tenderLength = (float)(ModelHandler.SierraTenderWheel.Model.GetSize().height * Math.PI);

            // First carriage is tender...
            var tender = Train.Carriages[1];

            AddWheel(Train, ModelHandler.SierraFrontWheel, "fwheel_", 2, _frontWheels);
            AddWheel(Train, ModelHandler.SierraDrivingWheel, "dwheel_", 3, _driveWheels);
            AddWheel(tender, ModelHandler.SierraTenderWheel, "twheel_", 4, _tenderWheels);

            void AddWheel(Vehicle vehicle, CustomModel wheelModel, string boneBase, int boneNumber, AnimatePropsHandler wheelHandler)
            {
                BoneUtils.ProcessMultipleBones(boneBase, boneNumber, bone =>
                {
                    // TODO: Temporary solution, model needs to be rotated
                    var rotOffset = Vector3.Zero;
                    if (wheelModel == ModelHandler.SierraDrivingWheel)
                        rotOffset.X = 85;

                    var wheelProp = new AnimateProp(wheelModel, vehicle, bone, Vector3.Zero, rotOffset);

                    wheelHandler.Add(wheelProp);
                });
                wheelHandler.SpawnProp();
            }
        }

        public override void Update()
        {
            // 2,3 wheel turn = 2.3 * 360 = 828~ degrees
            // tick calls 1/fps times per second, so 828 / 60 = 13,8 degrees per tick

            // Calculate wheel rotations per frame

            // Drive wheels
            float frameAngle = Physx.DriveWheelSpeed.AngularSpeed(_driveLength, _driveWheels[0].SecondRotation.X);

            _driveWheels.SetRotation(FusionEnums.Coordinate.X, frameAngle);

            // Front wheels
            frameAngle = Physx.VisualSpeed.AngularSpeed(_frontLength, _frontWheels[0].SecondRotation.X);

            _frontWheels.SetRotation(FusionEnums.Coordinate.X, frameAngle);

            // Tender wheels
            frameAngle = Physx.VisualSpeed.AngularSpeed(_tenderLength, _tenderWheels[0].SecondRotation.X);

            _tenderWheels.SetRotation(FusionEnums.Coordinate.X, frameAngle);
        }

        public override void Dispose()
        {
            _driveWheels.Dispose();
            _frontWheels.Dispose();
            _tenderWheels.Dispose();
        }
    }
}
