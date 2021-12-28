using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using RageComponent;
using RageComponent.Core;
using System.Drawing;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>
    /// Defines behaviour of train cab camera.
    /// </summary>
    public class CameraComponent : Component
    {
        /// <summary>
        /// Gets or sets value that controls if crosshair blinks or not.
        /// </summary>
        public static bool BlinkCrosshairThisFrame { get; set; }

        public static PointF CenterScreen => _centerScreen;

        private float _cabCameraYAxis;
        private float _prevTrainAngle = 0f;

        private DrivingComponent driving;
        private Camera _cabCamera;
        private static Pool<Camera> _cameraPool;
        private readonly Train _train;
        private static readonly TextElement _crosshair = new TextElement(".", default, 1f, Color.White, GTA.UI.Font.HouseScript);
        private static readonly PointF _centerScreen = new PointF(Screen.Width / 2, Screen.Height / 2 - 30);
        private static float _currentLevel = 0;
        private static bool _isLevelGoingUp = false;

        public CameraComponent(ComponentCollection components) : base(components)
        {
            _train = GetParent<Train>();
        }

        public override void Start()
        {
            driving = Components.GetComponent<DrivingComponent>();

            // We do that in start because it's unsafe 
            // to access gta stuff in constructors
            if(_cameraPool == null)
            {
                _cameraPool = new Pool<Camera>(
                    size: 1,
                    fill: () => World.CreateCamera(default, default, 65))
                {
                    OnDispose = x => x.Delete()
                };
            }

            driving.OnEnter += () =>
            {
                // Restore camera rotation too, cuz otherwise
                // prevAngle will be 0 and script will think
                // that train rotate and camera will offset
                _prevTrainAngle = _train.Rotation.Z;

                Vector3 cameraPos = _train.Bones["seat_dside_f"]
                    .GetRelativeOffsetPosition(new Vector3(0, -0.1f, 0.75f));

                // For some reason this method being invoked twice or something?
                // and it causes no free objects in pool exception
                // So as "temporary" hack...
                if (_cabCamera != null)
                    return;

                _cabCamera = _cameraPool.Get();
                _cabCamera.AttachTo(_train, cameraPos);

                SetupCamera();
            };
            driving.OnLeave += () =>
            {
                FreeCamera();
                GPlayer.IsVisible = true;
            };
        }

        public override void Update()
        {
            if (!driving.IsControlledByPlayer || _cabCamera == null)
                return;

            if(!FusionUtils.IsCameraInFirstPerson())
            {
                if (World.RenderingCamera.Equals(_cabCamera))
                {
                    World.RenderingCamera = null;
                    GPlayer.IsVisible = true;
                }
                return;
            }

            if(!World.RenderingCamera.Equals(_cabCamera))
            {
                World.RenderingCamera = _cabCamera;
                SetupCamera();
            }
            GPlayer.IsVisible = false;

            // Show crosshair for easier interaction with controls
            _crosshair.Position = _centerScreen;
            _crosshair.Draw();

            if(BlinkCrosshairThisFrame)
            {
                // Cycle from 0 to 255 and so on

                if(_currentLevel >= 255 && _isLevelGoingUp)
                {
                    _isLevelGoingUp = false;
                }
                else if(_currentLevel <= 0 && !_isLevelGoingUp)
                {
                    _isLevelGoingUp = true;
                }

                float add = Game.LastFrameTime * 1000;

                _currentLevel += _isLevelGoingUp ? add : -add;
                _currentLevel = _currentLevel.Clamp(0, 255);

                _crosshair.Color = Color.FromArgb((int) _currentLevel, Color.White);
            }
            else
            {
                _crosshair.Color = Color.White;
            }
            BlinkCrosshairThisFrame = false;

            // Zoom - Middle Mouse Btn
            Game.DisableControlThisFrame(Control.Phone);
            
            float povTo = 65;
            if(Game.IsControlPressed(Control.Phone))
            {
                povTo = 30;
            }
            _cabCamera.FieldOfView = FusionUtils.Lerp(_cabCamera.FieldOfView, povTo, Game.LastFrameTime * 4);

            // When train moves and rotates, camera moves with it
            // but rotation remains unchanged. So we have to
            // calculate on how much train rotated this frame
            // to keep rotation synced with train

            float trainAngle = _train.Rotation.Z;

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
            _cabCamera.Direction = _train.Quaternion * Vector3.RelativeFront;

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
