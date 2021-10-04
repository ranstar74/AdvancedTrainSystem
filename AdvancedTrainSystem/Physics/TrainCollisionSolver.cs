using AdvancedTrainSystem.Train;
using FusionLibrary.Extensions;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Physics
{
    /// <summary>
    /// This class solves collision between two <see cref="CustomTrain"/>.
    /// </summary>
    internal class TrainCollisionSolver
    {
        /// <summary>
        /// Pair of two colliding trains.
        /// </summary>
        private static readonly List<(CustomTrain, CustomTrain)> CollidingTrains = new List<(CustomTrain, CustomTrain)>();

        /// <summary>
        /// Marks that collision of these trains needs to be solved.
        /// </summary>
        /// <param name="train1">First collided train.</param>
        /// <param name="train2">Second collided train.</param>
        public static void Append(CustomTrain train1, CustomTrain train2)
        {
            if (!CollidingTrains.Contains((train1, train2)) && !CollidingTrains.Contains((train2, train1)))
            {
                CollidingTrains.Add((train1, train2));
            }
        }

        /// <summary>
        /// Marks that collision of these trains no longer need to be solved.
        /// </summary>
        /// <param name="train1">First collided train.</param>
        /// <param name="train2">Second collided train.</param>
        public static void Remove(CustomTrain train1, CustomTrain train2)
        {
            if (CollidingTrains.Contains((train1, train2)) || CollidingTrains.Contains((train2, train1)))
            {
                CollidingTrains.Remove((train1, train2));
                CollidingTrains.Remove((train2, train1));
            }
        }

        /// <summary>
        /// Resolves all existing collision of this frame.
        /// </summary>
        public static void Update()
        {
            //GTA.UI.Screen.ShowSubtitle(CollidingTrains.Count.ToString());
            for(int i = 0; i < CollidingTrains.Count; i++)
            {
                (CustomTrain train1, CustomTrain train2) = CollidingTrains[i];

                var impulse1 = SolveElasticCollision(train1, train2);
                var impulse2 = SolveElasticCollision(train2, train1);

                train1.SpeedComponent.ApplyTrackForce(impulse1);
                train2.SpeedComponent.ApplyTrackForce(impulse2);

                //GTA.UI.Screen.ShowSubtitle(
                //    $"S1: {train1.SpeedComponent.TrackSpeed} S2: {train2.SpeedComponent.TrackSpeed} I1: {train1.TrainHead.Speed} I2: {train2.TrainHead.Speed}");
            }
        }

        /// <summary>
        /// Solves elastic collision by calculating impulse that needs to be applied on this train.
        /// </summary>
        /// <param name="train1">Train, elastic collision of which needs to be solved.</param>
        /// <param name="train2">Train, train1 colliding with.</param>
        private static float SolveElasticCollision(CustomTrain train1, CustomTrain train2)
        {
            // Velocity of colliding object is:
            // (M1 * V1 + M2 * V2) / M1 + M2

            // TODO: Take mass of all entities into account
            // For now we'd assume that mass of train is 1

            float speed1 = train1.SpeedComponent.TrackSpeed;
            float speed2 = train2.SpeedComponent.TrackSpeed;

            float impulse = -(speed1 - ((speed1 + speed2) / 2));

            return impulse;
        }

        /// <summary>
        /// Separates two trains when one getting inside of other.
        /// </summary>
        /// <param name="train1">First colliding train.</param>
        /// <param name="train2">Second colliding train.</param>
        private static void SolveIntersection(CustomTrain train1, CustomTrain train2)
        {
            // Left just in case

            // Detect intersection distance
            // TODO: Implement proper detection with model bounding box
            var distances = new List<float>()
            {
                train1.TrainHead.FrontPosition.DistanceToSquared(train2.TrainHead.FrontPosition),
                train1.TrainHead.FrontPosition.DistanceToSquared(train2.TrainEnd.RearPosition),
                train1.TrainEnd.RearPosition.DistanceToSquared(train2.TrainHead.FrontPosition),
                train1.TrainEnd.RearPosition.DistanceToSquared(train2.TrainEnd.RearPosition),
            };

            // Find smallest distance
            float smallestSquareDistance = distances[0];
            for (int i = 1; i < distances.Count; i++)
            {
                if (distances[i] < smallestSquareDistance)
                    smallestSquareDistance = distances[i];
            }

            float intersection = (float)Math.Sqrt(smallestSquareDistance);

            // Now we need to find train that got inside others train, in most cases it will be
            // train with higher speed
            var targetTrain = train1.SpeedComponent.AbsoluteSpeed > train2.SpeedComponent.AbsoluteSpeed ? train1 : train2;
            var nonTargetTrain = targetTrain == train1 ? train2 : train1;

            // But in which direction we will push train out?
            if (targetTrain.TrainHead.IsBehind(nonTargetTrain.TrainHead))
                intersection *= -1;

            targetTrain.SpeedComponent.Move(intersection / 5);

            //GTA.UI.Screen.ShowSubtitle($"I: {intersection} T:{targetTrain.IsPlayerDriving} LF: {targetTrain.SpeedComponent.LastFrameAcceleration}", 1);
        }
    }
}