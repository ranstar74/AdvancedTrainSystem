using RageComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles train coupling.
    /// </summary>
    public class CoupleComponent : Component<CustomTrain>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Start()
        {
            Base.CollisionComponent.OnCouple += (train, s1, s2) =>
            {
                //  TOTAL FORMULA EQUALS: S1 + I(MAX(S2 - S1, 0))
                //  invert = train1.dir != train2.dir
                // Lets test it in all conditions:
                // 
                //  Condition 1: S1 must be > S2
                // --- > --- >
                // invert = false
                // S1 = 50
                // S2 = 48
                // E1 = 50 + (48 - 50) = 50 + 0 = 50 OK
                // E2 = 48 + (50 - 48) = 48 + 2 = 50 OK
                // - Success
                // S1 = 48
                // S2 = 50
                // E1 = 48 + (50 - 48) = 48 + 2 = 50 OK (IMPOSSIBLE CONDITION WITH PUSHING)
                // E2 = 50 + (48 - 50) = 50 + 0 = 50 OK

                // Basically adjusts distance between trains on couple.

                //var S1 = Base.Speed;
                //var S2 = train.Speed;
                //var speedDiff = Math.Max(S2 - S1, 0);

                //if (train.Direction != Base.Direction)
                //    speedDiff *= -1;

                //Base.TrainHead.Speed += speedDiff;

                // Velocity of colliding object is:
                // (M1 * V1 + M2 * V2) / M1 + M2

                // TODO: Take mass of all carriages into account
                // For now we'd assume that mass of train is 1

                var force = -(s1 - ((s1 + s2) / 2));
                Base.SpeedComponent.ApplyForce(force);
                var force2 = -(s2 - ((s2 + s1) / 2));
                train.SpeedComponent.ApplyForce(force2);

                Debug.Log(this, Base.Guid, s1, s2, force);
            };
        }
    }
}
