using AdvancedTrainSystem.Core.Extensions;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>
    /// Defines behaviour of train cab camera.
    /// </summary>
    public class CameraComponent : Component
    {
        public Camera sCabCamera;

        /// <summary>
        /// Current Y angle of the <see cref="sCabCamera"/>.
        /// </summary>
        private static float cabCameraYAxis;

        private readonly Train train;
        private DrivingComponent driving;

        private float prevTrainAngle = 0f;

        public CameraComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            driving = Components.GetComponent<DrivingComponent>();

            sCabCamera = World.CreateCamera(Vector3.Zero, Vector3.Zero, 65);

            Vector3 cameraPos = ((Vehicle)train).Bones["seat_dside_f"]
                .GetRelativeOffsetPosition(new Vector3(0, -0.1f, 0.75f));
            sCabCamera.AttachTo(train, cameraPos);

            driving.OnEnter += () =>
            {
                // Restore camera rotation too, cuz otherwise
                // prevAngle will be 0 and script will think
                // that train rotate and camera will offset
                prevTrainAngle = train.Rotation.Z;
            };
        }

        public override void Update()
        {
            if (FusionUtils.IsCameraInFirstPerson() && train.Driver == GPlayer)
            {
                // Make player transparent cuz cab camera will interference with player model
                GPlayer.IsVisible = false;

                if (World.RenderingCamera != sCabCamera)
                {
                    World.RenderingCamera = sCabCamera;

                    // Align camera direction with train direction
                    sCabCamera.Direction = train.Quaternion * Vector3.RelativeFront;

                    // Otherwise direction doesn't apply...
                    Script.Wait(1);
                }

                // When train moves and rotates, camera moves with it
                // but rotation remains unchanged. So we have to
                // calculate on how much train rotated this frame
                // to keep rotation synced with train

                float trainAngle = train.Rotation.Z;

                // Get input from controller and rotate camera

                var inputX = Game.GetControlValueNormalized(Control.LookLeft) * 5;
                var inputY = Game.GetControlValueNormalized(Control.LookUp) * 5;

                // Clamp vertical axis so we can't rotate camera more than 80 degrees up / down
                cabCameraYAxis -= inputY;
                cabCameraYAxis = cabCameraYAxis.Clamp(-80, 80);

                var newRotation = sCabCamera.Rotation;
                newRotation.Z -= inputX - (trainAngle - prevTrainAngle);
                newRotation.X = cabCameraYAxis;

                sCabCamera.Rotation = newRotation;

                prevTrainAngle = trainAngle;
            }
            else if(World.RenderingCamera == sCabCamera)
            {
                GPlayer.IsVisible = true;
                World.RenderingCamera = null;
            }
        }

        public override void Dispose()
        {
            if (World.RenderingCamera == sCabCamera)
                World.RenderingCamera = null;

            sCabCamera.Delete();
            GPlayer.IsVisible = true;
        }
    }
}
