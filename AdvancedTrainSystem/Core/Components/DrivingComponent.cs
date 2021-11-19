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

        private readonly Train train;

        /// <summary>
        /// Enter / leave train input.
        /// </summary>
        private readonly NativeInput enterInput = new NativeInput(Control.Enter);

        public DrivingComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            enterInput.OnControlPressed = OnEnterVehiclePressed;
        }

        private void OnEnterVehiclePressed()
        {
            if (Game.Player.Character.IsInAdvancedTrain())
            {
                Leave();

                return;
            }

            // Check if player is close enough to seat
            float distanceToSeat = Game.Player.Character.Position.DistanceToSquared(((Vehicle)train).Bones["cab_center"].Position);
            if (distanceToSeat > 3.5 * 3.5)
                return;

            Enter();
        }

        /// <summary>
        /// Will teleport player in train.
        /// </summary>
        public void Enter()
        {
            Game.Player.Character.Task.WarpIntoVehicle(train.TrainLocomotive.HiddenVehicle, VehicleSeat.Driver);

            OnEnter?.Invoke();
        }

        /// <summary>
        /// Will force player to leave the train.
        /// </summary>
        public void Leave()
        {
            Game.Player.Character.Task.LeaveVehicle();

            // TODO: Get relative position before entering train instead of hardcoded position
            Game.Player.Character.PositionNoOffset = ((Vehicle)train).GetOffsetPosition(new Vector3(-0.016f, -4.831f, 2.243f));

            OnLeave?.Invoke();
        }
    }
}
