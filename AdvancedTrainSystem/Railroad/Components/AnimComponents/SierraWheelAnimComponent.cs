using AdvancedTrainSystem.Core.Components.Abstract.AnimComponents;
using AdvancedTrainSystem.Core.Utils;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;
using System.Linq;

namespace AdvancedTrainSystem.Railroad.Components.AnimComponents
{
    public class SierraWheelAnimComponent : AnimComponent
    {
        /// <summary>
        /// Gets angle of the driving wheel.
        /// </summary>
        public float DrivingWheelAngle => DriveWheels[0].SecondRotation.X;

        /// <summary>
        /// Length of the drive wheel.
        /// </summary>
        public float DriveWheelLength => _driveLength;

        private readonly AnimatePropsHandler DriveWheels = new AnimatePropsHandler();
        private readonly AnimatePropsHandler FrontWheels = new AnimatePropsHandler();
        private readonly AnimatePropsHandler TenderWheels = new AnimatePropsHandler();

        private float _driveLength;
        private float _frontLength;
        private float _tenderLength;

        public SierraWheelAnimComponent(ComponentCollection components) : base(components)
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
            var tender = train.Carriages.First();

            AddWheel(train, ModelHandler.SierraFrontWheel, "fwheel_", 2, FrontWheels);
            AddWheel(train, ModelHandler.SierraDrivingWheel, "dwheel_", 3, DriveWheels);
            AddWheel(tender, ModelHandler.SierraTenderWheel, "twheel_", 4, TenderWheels);

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
            float frameAngle = physx.DriveWheelSpeed.AngularSpeed(_driveLength, DriveWheels[0].SecondRotation.X);

            DriveWheels.SetRotation(FusionEnums.Coordinate.X, frameAngle);

            // Front wheels
            frameAngle = physx.Speed.AngularSpeed(_frontLength, FrontWheels[0].SecondRotation.X);

            FrontWheels.SetRotation(FusionEnums.Coordinate.X, frameAngle);

            // Tender wheels
            frameAngle = physx.Speed.AngularSpeed(_tenderLength, TenderWheels[0].SecondRotation.X);

            TenderWheels.SetRotation(FusionEnums.Coordinate.X, frameAngle);
        }

        public override void Dispose()
        {
            DriveWheels.Dispose();
            FrontWheels.Dispose();
            TenderWheels.Dispose();
        }
    }
}
