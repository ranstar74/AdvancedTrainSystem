using AdvancedTrainSystem.Core.Extensions;
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
        }

        public override void Update()
        {
            // In case if player exits train some other way
            if (isPlayerDriving && GPlayer.CurrentVehicle != train.TrainLocomotive.HiddenVehicle)
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

            if (Game.Player.Character.IsInAdvancedTrain())
            {
                Leave();

                return;
            }

            // Check if player is close enough to seat
            Vector3 seatPos = ((Vehicle)train).Bones["seat_dside_f"].Position;

            float distanceToSeat = Game.Player.Character.Position.DistanceToSquared(seatPos);
            if (distanceToSeat > 3.5 * 3.5)
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
            Game.Player.Character.Task.WarpIntoVehicle(train.TrainLocomotive.HiddenVehicle, VehicleSeat.Driver);

            EnterEvents();
        }

        /// <summary>
        /// Will force player to leave the train.
        /// </summary>
        public void Leave()
        {
            Game.Player.Character.Task.WarpOutOfVehicle(train.TrainLocomotive.HiddenVehicle);

            // TODO: Get relative position before entering train instead of hardcoded position
            Game.Player.Character.PositionNoOffset = ((Vehicle)train).GetOffsetPosition(new Vector3(-0.016f, -4.831f, 2.243f));

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
