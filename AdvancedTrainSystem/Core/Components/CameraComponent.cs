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
        private static readonly Camera sCabCamera;

        /// <summary>
        /// Current Y angle of the <see cref="sCabCamera"/>.
        /// </summary>
        private static float cabCameraYAxis;

        private readonly Train train;
        private DrivingComponent driving;

        private float prevTrainAngle = 0f;

        static CameraComponent()
        {
            sCabCamera = World.CreateCamera(Vector3.Zero, Vector3.Zero, 65);
        }

        public CameraComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();

            SetCabCamera();
        }

        public override void Start()
        {
            driving = Components.GetComponent<DrivingComponent>();

            driving.OnEnter += () =>
            {
                // Make player transparent cuz cab camera will interference with player model
                Game.Player.Character.IsVisible = false;
            };
            driving.OnLeave += () =>
            {
                Game.Player.Character.IsVisible = true;

                World.RenderingCamera = null;
            };

            // In case if script was reloaded we wan't to
            // restore cab camera
            if (Game.Player.Character.IsInAdvancedTrain())
            {
                // Restore camera rotation too, cuz otherwise
                // prevAngle will be 0 and script will think
                // that train rotate and camera will offset
                prevTrainAngle = train.Rotation.Z;

                SetCabCamera();
            }
        }

        public override void Update()
        {
            if (World.RenderingCamera != sCabCamera)
                return;

            if (FusionUtils.IsCameraInFirstPerson())
            {
                // When train moves and rotates, camera moves with it
                // but rotation remains unchanged. So we have to
                // calculate on how much train rotated this frame
                // to keep rotation synced with train

                float trainAngle = train.Rotation.Z;

                trainAngle -= prevTrainAngle;

                // Get input from controller and rotate camera

                var inputX = Game.GetControlValueNormalized(Control.LookLeft) * 5;
                var inputY = Game.GetControlValueNormalized(Control.LookUp) * 5;

                // Clamp vertical axis so we can't rotate camera more than 80 degrees up / down
                cabCameraYAxis -= inputY;
                cabCameraYAxis = cabCameraYAxis.Clamp(-80, 80);

                var newRotation = sCabCamera.Rotation;
                newRotation.Z -= inputX - trainAngle;
                newRotation.X = cabCameraYAxis;

                sCabCamera.Rotation = newRotation;

                prevTrainAngle = trainAngle;
            }
            else
            {
                Game.Player.Character.IsVisible = true;

                if (World.RenderingCamera == sCabCamera)
                    World.RenderingCamera = null;
            }
        }

        public override void Dispose()
        {
            if (World.RenderingCamera == sCabCamera)
                World.RenderingCamera = null;

            Game.Player.Character.IsVisible = true;

            sCabCamera?.Delete();
        }

        private void SetCabCamera()
        {
            sCabCamera.Position = train.Position;
            sCabCamera.Rotation = train.Rotation;

            Vector3 cameraPos = ((Vehicle)train).Bones["seat_dside_f"].GetRelativeOffsetPosition(new Vector3(0, -0.1f, 0.75f));
            sCabCamera.AttachTo(train, cameraPos);

            // Align camera direction with train direction
            sCabCamera.Direction = train.Quaternion * Vector3.RelativeFront;
        }
    }
}
