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
        public bool IsControlledByPlayer => isPlayerDriving;

        private DerailComponent _derail;

        private readonly Train train;
        private readonly NativeInput enterInput = new NativeInput(Control.Enter);
        private bool isPlayerDriving = false;
        private int enterDelay = 0;

        public DrivingComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            enterInput.OnControlPressed = OnEnterVehiclePressed;

            // Restore enter after reload
            if (GPlayer.CurrentVehicle == train.TrainLocomotive.HiddenVehicle)
                EnterEvents();

            _derail = Components.GetComponent<DerailComponent>();

            _derail.OnDerail += () =>
            {
                if(train.Driver == GPlayer)
                {
                    GPlayer.Task.WarpIntoVehicle(train, VehicleSeat.Driver);
                }
            };
        }

        public override void Update()
        {
            // In case if player exits train some other way
            if (!train.IsPlayerDriving)
            {
                // To prevent fake alarm
                if(enterDelay < Game.GameTime)
                    LeaveEvents();
            }
        }

        private void OnEnterVehiclePressed()
        {
            // Check if some time passed since player entered train
            if (enterDelay > Game.GameTime)
                return;

            if (isPlayerDriving)
            {
                Leave();

                return;
            }

            // Check if player is close enough to seat
            Vector3 seatPos = train.Bones["seat_dside_f"].Position;

            float distanceToSeat = GPlayer.Position.DistanceToSquared(seatPos);
            if (distanceToSeat > 2f)
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
            while(train.TrainLocomotive.HiddenVehicle.Handle == 0)
            {
                GTA.UI.Screen.ShowSubtitle("Handle is invalid.", 1);

                Script.Yield();
            }

            GPlayer.Task.WarpIntoVehicle(train.GetActiveLocomotiveVehicle(), VehicleSeat.Driver);

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

            isPlayerDriving = true;
            OnEnter?.Invoke();
        }

        private void LeaveEvents()
        {
            SetDelay();

            isPlayerDriving = false;
            OnLeave?.Invoke();
        }

        private void SetDelay()
        {
            // Don't allow player to instantly leave train as if he holds enter button
            // it starts to rapidly switching between enter/leave
            enterDelay = Game.GameTime + 350;
        }
    }
}
