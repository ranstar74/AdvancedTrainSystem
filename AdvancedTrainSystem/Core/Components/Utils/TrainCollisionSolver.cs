using System.Collections.Generic;

namespace AdvancedTrainSystem.Core.Components.Utils
{
    /// <summary>
    /// This class solves collision between two <see cref="Train.Train"/>.
    /// </summary>
    internal static class TrainCollisionSolver
    {
        /// <summary>
        /// Pair of two colliding trains.
        /// </summary>
        private static readonly List<(Train, Train)> CollidingTrains = new List<(Train, Train)>();

        /// <summary>
        /// Marks that collision of these trains needs to be solved.
        /// </summary>
        /// <param name="train1">First collided train.</param>
        /// <param name="train2">Second collided train.</param>
        public static void Append(Train train1, Train train2)
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
        public static void Remove(Train train1, Train train2)
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
            for(int i = 0; i < CollidingTrains.Count; i++)
            {
                (Train train1, Train train2) = CollidingTrains[i];

                var impulse1 = SolveElasticCollision(train1, train2);
                var impulse2 = SolveElasticCollision(train2, train1);

                train1.Components.Physx.ApplyTrackForce(impulse1);
                train2.Components.Physx.ApplyTrackForce(impulse2);

                //GTA.UI.Screen.ShowSubtitle(
                //    $"S1: {train1.SpeedComponent.TrackSpeed} S2: {train2.SpeedComponent.TrackSpeed} I1: {train1.TrainHead.Speed} I2: {train2.TrainHead.Speed}");
            }
        }

        /// <summary>
        /// Solves elastic collision by calculating impulse that needs to be applied on this train.
        /// </summary>
        /// <param name="train1">Train, elastic collision of which needs to be solved.</param>
        /// <param name="train2">Train, train1 colliding with.</param>
        private static float SolveElasticCollision(Train train1, Train train2)
        {
            // Velocity of colliding object is:
            // (M1 * V1 + M2 * V2) / M1 + M2

            // TODO: Take mass of all entities into account
            // For now we'd assume that mass of train is 1

            float speed1 = train1.Components.Physx.TrackSpeed;
            float speed2 = train2.Components.Physx.TrackSpeed;

            float impulse = -(speed1 - ((speed1 + speed2) / 2));

            return impulse;
        }
    }
}
