using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Extensions;
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
        /// Invokes on train collision with other vehicle.
        /// </summary>
        public OnCollision OnCollision { get; set; }

        /// <summary>
        /// List of closes vehicles to train, not including carriages on this train. 
        /// Updates every 250ms.
        /// </summary>
        private readonly List<Vehicle> _closestVehicles = new List<Vehicle>();

        /// <summary>
        /// Predict position of train head in next frame based on speed.
        /// </summary>
        public Vector3 HeadPositionNextFrame { get; private set; }

        /// <summary>
        /// Next closest vehicles list update time.
        /// </summary>
        private float _closestVehiclesUpdateTime;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {
            GetClosestVehicles();
            ProcessCollision();
        }

        /// <summary>
        /// Gets various collision information.
        /// </summary>
        private void ProcessCollision()
        {
            // No point to check collision after derail because visible vehicles
            // support native game collision
            if (Base.DerailComponent.IsDerailed)
                return;

            // When entity speed raises, position starts drift back so we're trying to compensate that
            var driftOffset = Base.TrainHeadVisible.ForwardVector * Base.Speed * Game.LastFrameTime;
            var headEdgePosition = Base.TrainHeadVisible.FrontPosition + Vector3.WorldUp + driftOffset;

            // Position in next frame = current position + speed * deltaTime
            HeadPositionNextFrame = headEdgePosition + driftOffset;

            // Process every closest vehicle
            for (int i = 0; i < _closestVehicles.Count; i++)
            {
                var closestVehicle = _closestVehicles[i];

                // Check every train carriage
                for (int k = 0; k < Base.Carriages.Count; k++)
                {
                    var carriage = Base.Carriages[k];

                    bool hasCarriageCollided = false;
                    float collidedEntitySpeed = 0;

                    // Collision with other custom train
                    if(closestVehicle.IsCustomTrain())
                    {
                        // Since trains in gta doesn't collide with other trains, we have
                        // to predicate when train will collide with other train, otherwise
                        // one train will go into another train and on visible model detach 
                        // game will have to teleport thems because entity can't be inside another entity

                        CustomTrain closestCustomTrain = CustomTrain.Find(closestVehicle);

                        // Calcualte distance from other train head to this train head
                        Vector3 closestHeadPosition = closestCustomTrain.CollisionComponent.HeadPositionNextFrame;
                        float distanceBetweenTrains = closestHeadPosition.DistanceToSquared(HeadPositionNextFrame);

                        // With higher speed there's higher chance that train will "get inside" another train
                        // so we're trying to minimize that with higher collision detection distance
                        if (distanceBetweenTrains < Base.Speed / 10)
                        {
                            collidedEntitySpeed = closestCustomTrain.Speed;
                            hasCarriageCollided = true;
                        }
                    }

                    // Collision with vehicle
                    if (closestVehicle.IsTouching(carriage.InvisibleVehicle))
                    {
                        collidedEntitySpeed = closestVehicle.Speed;
                        hasCarriageCollided = true;
                    }

                    // Calculate energy of colliding vehicles
                    float speedDifference = Math.Abs(collidedEntitySpeed - Base.SpeedComponent.Speed);
                    float mass = HandlingData.GetByVehicleModel(closestVehicle.Model).Mass;
                    float energy = mass * collidedEntitySpeed;

                    // TODO: Couple otherwise
                    if (energy > 100000)
                        hasCarriageCollided = true;

                    if (hasCarriageCollided)
                    {
                        OnCollision?.Invoke(new CollisionInfo(carriage, closestVehicle, speedDifference));
                    }
                }
            }
        }

        /// <summary>
        /// Gets closest vehicles to train, including other trains.
        /// </summary>
        private void GetClosestVehicles()
        {
            // There's no point to update closest vehicles every tick
            // cuz its makes big performance impact + theres no way
            // to vehicle to appear in 120m radius and collide with train within 250ms
            if(_closestVehiclesUpdateTime > Game.GameTime)
            {
                _closestVehicles.Clear();
                var closestVehicles = World.GetNearbyVehicles(Entity.Position, 120);

                // Remove vehicles that belong to this train
                for (int i = 0; i < closestVehicles.Length; i++)
                {
                    var vehicle = closestVehicles[i];

                    if (vehicle.Decorator().GetInt(Constants.TrainGuid) != Base.Guid)
                    {
                        _closestVehicles.Add(vehicle);
                    }
                }

                _closestVehiclesUpdateTime = Game.GameTime + 250;
            }
        }
    }
}
