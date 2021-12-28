using FusionLibrary;
using GTA;
using GTA.Math;
using RageComponent;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>
    /// Defines base enter / leave train actions.
    /// </summary>
    public class DrivingComponent : Component
    {
        /// <summary>
        /// Being invoked after player starts controlling train.
        /// </summary>
        public Action OnEnter { get; set; }

        /// <summary>
        /// Being invoked after player stops controlling train.
        /// </summary>
        public Action OnLeave { get; set; }

        /// <summary>
        /// Gets a value indicating whether the train is controlled by player.
        /// </summary>
        public bool IsControlledByPlayer => _isPlayerDriving;

        /// <summary>
        /// Gets a value indicating whether player is in the train.
        /// </summary>
        public bool IsInCab => _isInCab;

        private DerailComponent _derail;

        private readonly Train _train;
        private readonly NativeInput _enterInput = new NativeInput(Control.Enter);
        private bool _isPlayerDriving = false;
        private int _enterDelay = 0;
        private bool _isInCab;
        private float _distanceToSeat = 0f;

        public DrivingComponent(ComponentCollection components) : base(components)
        {
            _train = GetParent<Train>();
        }

        public override void Start()
        {
            _enterInput.OnControlPressed = OnEnterVehiclePressed;

            // Restore enter after reload
            if (GPlayer.CurrentVehicle == _train.TrainLocomotive.HiddenVehicle)
                EnterEvents();

            _derail = Components.GetComponent<DerailComponent>();

            _derail.OnDerail += () =>
            {
                if (_train.Driver == GPlayer)
                    return;

                GPlayer.Task.WarpIntoVehicle(_train, VehicleSeat.Driver);
            };
        }

        public override void Update()
        {
            // In case if player exits train some other way
            if (!_train.IsPlayerDriving)
            {
                // To prevent fake alarm
                if(_enterDelay < Game.GameTime)
                    LeaveEvents();
            }

            Vector3 seatPos = _train.Bones["seat_dside_f"].Position;
            _distanceToSeat = GPlayer.Position.DistanceToSquared(seatPos);
            _isInCab = _distanceToSeat < 5f;
        }

        private void OnEnterVehiclePressed()
        {
            // Check if some time passed since player entered train
            if (_enterDelay > Game.GameTime)
                return;

            if (_isPlayerDriving)
            {
                Leave();

                return;
            }

            // Check if player is close enough to seat
            if (_distanceToSeat > 2.5f)
                return;

            Enter();
        }

        /// <summary>
        /// Will teleport player in train.
        /// </summary>
        public void Enter()
        {
            // We are forced to use hidden vehicle as driving vehicle because
            // it provides collision, meaning if we disable it and use
            // collision of visible model it will be very bugged on speed.
            // plus because of that if we put player in visible model
            // game will move camera just under train.
            // Maybe custom camera is solution? Someday...
            
            // A temporary code for "zero coordinates" bug
            while(_train.TrainLocomotive.HiddenVehicle.Handle == 0)
            {
                GTA.UI.Screen.ShowSubtitle("Handle is invalid.", 1);

                Script.Yield();
            }

            GPlayer.Task.WarpIntoVehicle(_train.GetActiveLocomotiveVehicle(), VehicleSeat.Driver);

            EnterEvents();
        }

        /// <summary>
        /// Will force player to leave the train.
        /// </summary>
        public void Leave()
        {
            GPlayer.Task.LeaveVehicle();

            LeaveEvents();
        }

        private void EnterEvents()
        {
            SetDelay();

            _isPlayerDriving = true;
            OnEnter?.Invoke();

            _train.Blip.Alpha = 0;
        }

        private void LeaveEvents()
        {
            SetDelay();

            _isPlayerDriving = false;
            OnLeave?.Invoke();

            _train.Blip.Alpha = 255;
        }

        private void SetDelay()
        {
            // Don't allow player to instantly leave train as if he holds enter button
            // it starts to rapidly switching between enter/leave
            _enterDelay = Game.GameTime + 350;
        }
    }
}
