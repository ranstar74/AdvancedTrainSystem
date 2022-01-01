using AdvancedTrainSystem.Railroad.Components.Common;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    /// <summary>Hydrolocking piston will prevent it from moving, rapidly stopping train.</summary>
    public class Hydrobrake : Brake
    {
        public override float Efficiently => 3.5f;

        public override float Force => IsHydrolocked ? 1f : 0f;

        /// <summary>Gets or sets value that defines whether piston is hydrolocked or not.</summary>
        public bool IsHydrolocked { get; set; }

        public Hydrobrake(ComponentCollection components) : base(components)
        {

        }

        public override void EarlyUpdate()
        {
            if (IsHydrolocked)
            {
                Physx.DontApplyDriveForcesThisFrame = true;
                Physx.AreDriveWheelsLockedThisFrame = true;
            }
        }
    }
}
