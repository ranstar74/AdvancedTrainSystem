using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.UI;
using RageComponent.Core;
using System.Drawing;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Controls train cabin camera.</summary>
    public class CinematicCamera : TrainComponent
    {
        /// <summary>Gets or sets value that controls if crosshair blinks this frame or not.</summary>
        public static bool BlinkCrosshairThisFrame { get; set; }

        /// <summary>Gets point with coordinates of center of the screen.</summary>
        public static PointF CenterScreen => _centerScreen;

        private Camera _cabCamera;
        private float _cabCameraYAxis;
        private float _prevTrainAngle = 0f;

        private static readonly TextElement _crosshair;
        private static readonly PointF _centerScreen;
        private static float _currentLevel = 0;
        private static bool _isCrosshairBlinkGoingUp = false;

        private static Pool<Camera> _cameraPool;

        static CinematicCamera()
        {
            _crosshair = new TextElement(".", default, 1f, Color.White, GTA.UI.Font.HouseScript);
            _centerScreen = new PointF(Screen.Width / 2, Screen.Height / 2 - 30);
        }

        public CinematicCamera(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            // We do that in start because it's unsafe 
            // to access gta stuff in constructors
            if (_cameraPool == null)
            {
                _cameraPool = new Pool<Camera>(
                    size: 1,
                    fill: () => World.CreateCamera(default, default, 65))
                {
                    OnDispose = x => x.Delete()
                };
            }

            Driving.OnEnter += () =>
            {
                // Restore camera rotation too, cuz otherwise
                // prevAngle will be 0 and script will think
                // that train rotate and camera will offset
                _prevTrainAngle = Train.Rotation.Z;

                Vector3 cameraPos = Train.Bones["seat_dside_f"]
                    .GetRelativeOffsetPosition(new Vector3(0, -0.1f, 0.75f));

                // For some reason this method being invoked twice or something?
                // and it causes no free objects in pool exception
                // So as "temporary" hack...
                if (_cabCamera != null)
                    return;

                _cabCamera = _cameraPool.Get();
                _cabCamera.AttachTo(Train, cameraPos);

                SetupCamera();
            };
            Driving.OnLeave += () =>
            {
                FreeCamera();
                GPlayer.IsVisible = true;
            };
        }

        public override void Update()
        {
            UpdateCabCamera();
            UpdateCrosshair();
        }

        private void UpdateCrosshair()
        {
            if (!FusionUtils.IsCameraInFirstPerson())
                return;

            // Allow interaction only when player is in cab and holds no weapon
            if (!Driving.IsInCab || GPlayer.Weapons.Current.DisplayName != "WT_UNARMED")
                return;

            // Disable all fight/shot controls so it wont mess with interaction
            Game.DisableControlThisFrame(Control.Aim);
            Game.DisableControlThisFrame(Control.AccurateAim);
            Game.DisableControlThisFrame(Control.Attack);
            Game.DisableControlThisFrame(Control.Attack2);
            Game.DisableControlThisFrame(Control.MeleeAttack1);
            Game.DisableControlThisFrame(Control.MeleeAttack2);
            Game.DisableControlThisFrame(Control.VehicleAim);

            // Show crosshair for easier interaction with controls
            _crosshair.Position = _centerScreen;
            _crosshair.Draw();

            if (BlinkCrosshairThisFrame)
            {
                // Cycle from 0 to 255 and so on

                if (_currentLevel >= 255 && _isCrosshairBlinkGoingUp)
                {
                    _isCrosshairBlinkGoingUp = false;
                }
                else if (_currentLevel <= 0 && !_isCrosshairBlinkGoingUp)
                {
                    _isCrosshairBlinkGoingUp = true;
                }

                float add = Game.LastFrameTime * 1000;

                _currentLevel += _isCrosshairBlinkGoingUp ? add : -add;
                _currentLevel = _currentLevel.Clamp(0, 255);

                _crosshair.Color = Color.FromArgb((int)_currentLevel, Color.White);
            }
            else
            {
                _crosshair.Color = Color.White;
            }
            BlinkCrosshairThisFrame = false;
        }

        private void UpdateCabCamera()
        {
            if (!Driving.IsControlledByPlayer || _cabCamera == null)
                return;

            if (!FusionUtils.IsCameraInFirstPerson())
            {
                if (World.RenderingCamera.Equals(_cabCamera))
                {
                    World.RenderingCamera = null;
                    GPlayer.IsVisible = true;
                }
                return;
            }

            if (!World.RenderingCamera.Equals(_cabCamera))
            {
                World.RenderingCamera = _cabCamera;
                SetupCamera();
            }
            GPlayer.IsVisible = false;

            // Zoom - Middle Mouse Btn
            Game.DisableControlThisFrame(Control.Phone);

            float povTo = 65;
            if (Game.IsControlPressed(Control.Phone))
            {
                povTo = 30;
            }
            _cabCamera.FieldOfView = FusionUtils.Lerp(_cabCamera.FieldOfView, povTo, Game.LastFrameTime * 4);

            // When train moves and rotates, camera moves with it
            // but rotation remains unchanged. So we have to
            // calculate on how much train rotated this frame
            // to keep rotation synced with train

            float trainAngle = Train.Rotation.Z;

            // Get input from controller and rotate camera

            var inputX = Game.GetControlValueNormalized(Control.LookLeft) * 5;
            var inputY = Game.GetControlValueNormalized(Control.LookUp) * 5;

            // Clamp vertical axis so we can't rotate camera more than 80 degrees up / down
            _cabCameraYAxis -= inputY;
            _cabCameraYAxis = _cabCameraYAxis.Clamp(-80, 80);

            var newRotation = _cabCamera.Rotation;
            newRotation.Z -= inputX - (trainAngle - _prevTrainAngle);
            newRotation.X = _cabCameraYAxis;

            _cabCamera.Rotation = newRotation;

            _prevTrainAngle = trainAngle;
        }

        private void SetupCamera()
        {
            // Align camera direction with train direction
            _cabCamera.Direction = Train.Quaternion * Vector3.RelativeFront;

            // Otherwise direction doesn't apply
            Script.Yield();
        }

        private void FreeCamera()
        {
            if (_cabCamera != null)
            {
                _cabCamera.Detach();
                _cameraPool.Free(_cabCamera);
                _cabCamera = null;

                World.RenderingCamera = null;
            }
        }

        public override void Dispose()
        {
            GPlayer.IsVisible = true;

            FreeCamera();
        }

        public override void Reload()
        {
            _cameraPool.Dispose();
        }
    }
}
