using AdvancedTrainSystem.Core.Components.Physics;
using AdvancedTrainSystem.Extensions;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>
    /// Handles collision detection between trains.
    /// </summary>
    public class CollisionComponent : Component
    {
        /// <summary>
        /// Invokes on train collision with other vehicle.
        /// </summary>
        public Action OnCollision { get; set; }

        /// <summary>
        /// Predict position of train head in next frame based on speed.
        /// </summary>
        public Vector3 HeadPositionNextFrame { get; private set; }

        /// <summary>
        /// Whether train is coupled or pushing other train. 
        /// </summary>
        public bool IsTrainCoupled { get; private set; }

        /// <summary>
        /// <see cref="Game.FrameCount"/> when train was coupled.
        /// </summary>
        public int CoupleFrame { get; private set; }

        /// <summary>
        /// Next closest vehicles list update time.
        /// </summary>
        private float closestVehiclesUpdateTime = 0;

        /// <summary>
        /// List of closes vehicles to train, not including carriages on this train. 
        /// Updates every 250ms.
        /// </summary>
        private readonly List<Vehicle> closestVehicles = new List<Vehicle>();

        private readonly Train train;
        private PhysxComponent physx;
        private DerailComponent derail;

        public CollisionComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            physx = Components.GetComponent<PhysxComponent>();
            derail = Components.GetComponent<DerailComponent>();
        }

        public override void Update()
        {
            GetClosestVehicles();
            ProcessCollision();
            TrainCollisionSolver.Update();
        }

        /// <summary>
        /// Gets various collision information.
        /// </summary>
        private void ProcessCollision()
        {
            // No point to check collision after derail because visible vehicles
            // support native game collision
            if (derail.IsDerailed)
                return;

            // When entity speed raises, position starts drift back so we're trying to compensate that
            // I actually not sure how the hell it works, but trust me, it works
            // It can be tested manually by drawing line using World.DrawLine
            // if u just draw it on some bone position, it will drift with speed
            // but if u do this, it magically fixes it...
            var driftOffset = train.ForwardVector * train.Speed * Game.LastFrameTime;
            var headEdgePosition = train.FrontPosition + Vector3.WorldUp + driftOffset;

            // Position in next frame = current position + speed * deltaTime
            HeadPositionNextFrame = headEdgePosition + driftOffset;

            // TODO: Use game scanner
            // Process every closest vehicle
            for (int i = 0; i < closestVehicles.Count; i++)
            {
                var closestVehicle = closestVehicles[i];

                // Check every train carriage
                for (int k = 0; k < train.Carriages.Count; k++)
                {
                    var carriage = train.Carriages[k];

                    bool hasCarriageCollided = false;
                    float collidedEntitySpeed = 0;

                    // Check for collision with other custom train
                    var isClosestVehicleCustomTrain = closestVehicle.IsAts();
                    Train closestCustomTrain = null;
                    if (isClosestVehicleCustomTrain)
                    {
                        // Since trains in gta doesn't collide with other trains, we have
                        // to predicate when train will collide with other train, otherwise
                        // one train will go into another train and on visible model detach 
                        // game will have to teleport thems because entity can't be inside another entity

                        closestCustomTrain = closestVehicle.GetAtsByCarriage();

                        // Calcualte distance from other train head to this train head
                        Vector3 closestHeadPosition = closestCustomTrain.Components.Collision.HeadPositionNextFrame;
                        float distanceBetweenTrains = closestHeadPosition.DistanceToSquared(HeadPositionNextFrame);

                        // With higher speed there's higher chance that train will "get inside" another train
                        // so we're trying to minimize that with higher collision detection distance
                        if (distanceBetweenTrains < train.Speed / 10)
                        {
                            collidedEntitySpeed = closestCustomTrain.Speed;
                            hasCarriageCollided = true;
                        }

                    }

                    // Check for collision with vehicle
                    if (closestVehicle.IsTouching(carriage.HiddenVehicle))
                    {
                        collidedEntitySpeed = closestVehicle.Speed;
                        hasCarriageCollided = true;
                    }

                    // Don't process collision if carriage didn't collided
                    if (!hasCarriageCollided)
                        continue;

                    // Derail if energy is high, otherwise push

                    // Use direction-less speed for trains and regular for vehicles
                    float othersVehicleSpeed =
                        isClosestVehicleCustomTrain ? closestCustomTrain.TrackSpeed: closestVehicle.Speed;
                    if (CalculateKineticEnergy(othersVehicleSpeed, closestVehicle) > 150000)
                    {
                        OnCollision?.Invoke();
                    }
                    else
                    {
                        if (isClosestVehicleCustomTrain)
                        {
                            Vehicle vehicle = train.TrainLocomotive.HiddenVehicle;

                            IsTrainCoupled = vehicle.IsGoingTorwards(closestCustomTrain);

                            if (IsTrainCoupled)
                            {
                                TrainCollisionSolver.Append(train, closestCustomTrain);
                                CoupleFrame = Game.FrameCount;
                            }
                            else
                                TrainCollisionSolver.Remove(train, closestCustomTrain);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates energy of collision with other vehicle.
        /// </summary>
        /// <param name="otherVehicleSpeed">Speed of vehicle this train colliding with.</param>
        /// <param name="otherVehicle">Vehicle this train colliding with.</param>
        private float CalculateKineticEnergy(float otherVehicleSpeed, Vehicle otherVehicle)
        {
            float speedDifference = Math.Abs(otherVehicleSpeed - physx.AverageSpeed);

            float mass = HandlingData.GetByVehicleModel(otherVehicle.Model).Mass;
            
            return mass * speedDifference;
        }

        /// <summary>
        /// Gets closest vehicles to train, including other trains.
        /// </summary>
        private void GetClosestVehicles()
        {
            // TODO: Replace with scanner class

            // There's no point to update closest vehicles every tick
            // cuz its makes big performance impact + theres no way
            // to vehicle to appear in 120m radius and collide with train within 250ms
            if (closestVehiclesUpdateTime < Game.GameTime)
            {
                this.closestVehicles.Clear();
                var closestVehicles = World.GetNearbyVehicles(train.Position, 120);

                // Remove vehicles that belong to this train
                for (int i = 0; i < closestVehicles.Length; i++)
                {
                    var vehicle = closestVehicles[i];

                    if (vehicle.GetAtsHandle() != train.ComponentHandle)
                    {
                        this.closestVehicles.Add(vehicle);
                    }
                }
                closestVehiclesUpdateTime = Game.GameTime + 250;
            }
        }
    }
}
