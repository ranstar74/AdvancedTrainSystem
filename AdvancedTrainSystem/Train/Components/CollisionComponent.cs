using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Extensions;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles collision detection between trains.
    /// </summary>
    public class CollisionComponent : Component<CustomTrain>
    {
        /// <summary>
        /// Minimum speed of derailnment in m/s.
        /// </summary>
        public const float DerailMinSpeed = 13;

        /// <summary>
        /// Minimum angle difference between current and previous frames to derail.
        /// </summary>
        public const float DerailAngle = 0.5f;

        /// <summary>
        /// Invokes on train collision with other vehicle.
        /// </summary>
        public OnCollision OnCollision { get; set; }

        private Vector3 _previousForwardAngle = Vector3.Zero;
        private readonly List<Vehicle> _closestVehicles = new List<Vehicle>();
        private float _closestVehiclesUpdateTime;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Start()
        {
            OnCollision += DerailOnCollision;
        }

        private void DerailOnCollision(CollisionInfo colInfo)
        {
            if (!colInfo.CollidingVehicle.IsCustomTrain())
                return;

            var mass = HandlingData.GetByVehicleModel(colInfo.CollidingVehicle.Model).Mass;
            var energy = mass * colInfo.SpeedDifference;


            //if (energy > 50000)
            foreach(var carriage in Base.Carriages)
            {
                carriage.InvisibleVehicle.IsCollisionEnabled = false;
            }
            foreach (var carriage in Base.Carriages)
            {
                carriage.VisibleVehicle.Detach();

                if (Game.Player.Character.IsInVehicle(carriage.InvisibleVehicle))
                {
                    Game.Player.Character.Task.WarpIntoVehicle(carriage.VisibleVehicle, Game.Player.Character.SeatIndex);
                }
            }
            //colInfo.Carriage.Derail();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {
            ProcessDerail();
            GetClosestVehicles();
            ProcessCollision();



            //// Derail if train crashed with something heavy
            //if(Train.VisibleModel.HasCollided)
            //{
            //    var counter = 0;
            //    var closestEntities = World.GetNearbyEntities(Train.VisibleModel.Position, 20);
            //    for (int i = 0; i < closestEntities.Count(); i++)
            //    {
            //        if(closestEntities[i].HasCollided)
            //        {
            //            counter++;
            //        }
            //    }

            //    GTA.UI.Screen.ShowSubtitle($"Collided entities: {counter}");
            //}
        }

        /// <summary>
        /// Gets various collision information.
        /// </summary>
        private void ProcessCollision()
        {
            // TODO: Implement support of multiple colliding vehicles

            // Process every closest vehicle
            for (int i = 0; i < _closestVehicles.Count; i++)
            {
                var closestVehicle = _closestVehicles[i];

                // Check every train carriage
                for (int k = 0; k < Base.Carriages.Count; k++)
                {
                    var carriage = Base.Carriages[k];

                    // Check if vehicle colliding with carriage
                    if (closestVehicle.IsTouching(carriage.InvisibleVehicle))
                    {
                        // Calculate the speed difference between two vehicles
                        var speedDifference = Math.Abs(closestVehicle.Speed - Base.SpeedComponent.Speed);

                        OnCollision?.Invoke(new CollisionInfo(carriage, closestVehicle, speedDifference));
                    }
                }
            }

            //if (speedDifference < 3)
            //{
            //    if (closestVehicle.IsCustomTrain())

            //        var head = (Vehicle)Entity.FromHandle(headHandle);
            //    //GTA.UI.Screen.ShowSubtitle($"Handle: {headHandle}", 1);

            //    CustomTrain.Find(headHandle).Couple(Train.CustomTrain);
            //    //    var couplingTrain = train
            //    //NVehicle.SetTrainSpeed(train, Train.SpeedComponent.Speed);
            //}
        }

        /// <summary>
        /// Gets closest vehicles to train, including other trains.
        /// </summary>
        private void GetClosestVehicles()
        {
            // Get all closest vehicles every 250ms
            if (_closestVehiclesUpdateTime < Game.GameTime)
            {
                _closestVehicles.Clear();
                var closestVehicles = World.GetNearbyVehicles(Entity.Position, 100);

                // Remove vehicles that belong to this train
                for (int i = 0; i < closestVehicles.Length; i++)
                {
                    var vehicle = closestVehicles[i];

                    if (vehicle.Decorator().GetInt(Constants.TrainGuid) != Base.Guid)
                    {
                        _closestVehicles.Add(vehicle);
                    }
                }
                _closestVehiclesUpdateTime = Game.GameTime + 1;
            }
        }

        private void ProcessDerail()
        {
            // Derail if train going is too fast on sharp corner

            // We're basically comparing forward vector of previous frame and current frame
            // and if difference is too high and speed is higher than derailing minumum then train derails.
            var forwardVector = Entity.ForwardVector;
            if (Math.Abs(Base.SpeedComponent.Speed) >= DerailMinSpeed)
            {
                float angle = Vector3.Angle(forwardVector, _previousForwardAngle);

                if (angle >= DerailAngle)
                {
                    if (FusionUtils.Random.NextDouble() >= 0.3f)
                    {
                        //Derail();
                    }
                }
            }
            _previousForwardAngle = forwardVector;
        }
    }
}
