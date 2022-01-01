using FusionLibrary;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Controls train enter and leave actions.</summary>
    public class Driving : TrainComponent
    {
        /// <summary>Invoked when player enters train.</summary>
        public Action OnEnter { get; set; }

        /// <summary>Invoked when player leaves train.</summary>
        public Action OnLeave { get; set; }

        /// <summary>Gets a value indicating whether the train is controlled by player.</summary>
        public bool IsControlledByPlayer { get; private set; }

        /// <summary>Gets a value indicating whether player is in the train cab or near it.</summary>
        public bool IsInCab { get; private set; }

        private readonly NativeInput _enterInput = new NativeInput(Control.Enter);

        private int _enterDelay = 0;
        private float _distanceToSeat = 0f;

        public Driving(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            _enterInput.OnControlPressed = OnEnterVehiclePressed;

            // Restore enter after reload
            if (GPlayer.CurrentVehicle == Train.TrainLocomotive.HiddenVehicle)
            {
                EnterEvents();
            }

            Derail.OnDerail += () =>
            {
                if (IsControlledByPlayer)
                {
                    GPlayer.Task.WarpIntoVehicle(Train, VehicleSeat.Driver);
                }
            };
        }

        public override void Update()
        {
            Vector3 seatPos = Train.Bones["seat_dside_f"].Position;
            _distanceToSeat = GPlayer.Position.DistanceToSquared(seatPos);
            IsInCab = _distanceToSeat < 5f || IsControlledByPlayer;

            // In case if player exits train some other way
            if (!Train.IsPlayerDriving)
            {
                // To prevent fake alarm
                if(_enterDelay < Game.GameTime)
                    LeaveEvents();
            }
        }

        private void OnEnterVehiclePressed()
        {
            // Check if some time passed since player entered train
            if (_enterDelay > Game.GameTime)
                return;

            if (IsControlledByPlayer)
            {
                Leave();
                return;
            }

            // Check if player is close enough to seat
            if (_distanceToSeat < 2.5f)
            {
                Enter();
            }
        }

        /// <summary>Forces player to enter this train.</summary>
        public void Enter()
        {
            // We are forced to use hidden vehicle as driving vehicle because
            // it provides collision, meaning if we disable it and use
            // collision of visible model it will be very bugged on speed.
            // plus because of that if we put player in visible model
            // game will move camera just under train.
            // Maybe custom camera is solution? Someday...
            
            // A temporary code for "zero coordinates" bug
            while(Train.TrainLocomotive.HiddenVehicle.Handle == 0)
            {
                GTA.UI.Screen.ShowSubtitle("Handle is invalid.", 1);

                Script.Yield();
            }

            GPlayer.Task.WarpIntoVehicle(Train.GetActiveLocomotiveVehicle(), VehicleSeat.Driver);

            EnterEvents();
        }

        /// <summary>Forces player to leave this train.</summary>
        public void Leave()
        {
            GPlayer.Task.LeaveVehicle();

            LeaveEvents();
        }

        private void EnterEvents()
        {
            SetDelay();

            IsControlledByPlayer = true;
            OnEnter?.Invoke();

            Train.Blip.Alpha = 0;
        }

        private void LeaveEvents()
        {
            SetDelay();

            IsControlledByPlayer = false;
            OnLeave?.Invoke();

            Train.Blip.Alpha = 255;
        }

        private void SetDelay()
        {
            // Don't allow player to instantly leave train as if he holds enter button
            // it starts to rapidly switching between enter/leave
            _enterDelay = Game.GameTime + 350;
        }
    }
}
