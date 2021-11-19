using AdvancedTrainSystem.Railroad;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.Native;
using RageComponent;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>
    /// Handles train derailnment.
    /// </summary>
    public class DerailComponent : Component
    {
        /// <summary>
        /// Invokes on train derail.
        /// </summary>
        public Action OnDerail { get; set; }

        /// <summary>
        /// Whether train is derailed or not.
        /// </summary>
        public bool IsDerailed { get; private set; }

        /// <summary>
        /// Minimum speed of derailnment in m/s.
        /// </summary>
        private const float derailMinSpeed = 13;

        /// <summary>
        /// Minimum angle difference between current and previous frames to derail.
        /// </summary>
        private const float derailAngle = 0.5f;

        private readonly Train train;
        private Vector3 prevForwardAngle = Vector3.Zero;
        private PhysxComponent physx;

        /// <summary>
        /// Creates a new instance of <see cref="DerailComponent"/>.
        /// </summary>
        /// <param name="components"></param>
        public DerailComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            Components.GetComponent<CollisionComponent>().OnCollision += Derail;
            physx = Components.GetComponent<PhysxComponent>();
        }

        public override void Update()
        {
            ProcessSpeedDerail();
        }

        /// <summary>
        /// Derails train with all carriages.
        /// </summary>
        public void Derail()
        {
            if (IsDerailed)
                return;

            OnDerail?.Invoke();

            // TODO: Make player fly like out of cars in gta 4
            // Throw player out of train
            if (train.Driver == Game.Player.Character)
            {
                Game.Player.Character.Task.LeaveVehicle(LeaveVehicleFlags.BailOut);
                Game.Player.Character.Ragdoll(10, RagdollType.Balance);
            }

            // Process all carriages from locomotive to last one
            for (int i = 0; i < train.Carriages.Count; i++)
            {
                Vehicle carriage = train.Carriages[i];
                Vehicle hiddenVehicle = train.Carriages[i].HiddenVehicle;

                // TODO: Fix trailer attach, its also being assigned while creating
                //// Attach carriage as trailer to next carriage if theres one
                //if (carriage.Next != null)
                //{
                //    carriage.VisibleVehicle.AttachToTrailer(carriage.Next.VisibleVehicle, 130);
                //}

                // Detach visible vehicle from invisible one and re-apply velocity
                carriage.Detach();
                carriage.Velocity = hiddenVehicle.Velocity;

                // Delete invisible model as its not longer needed
                hiddenVehicle.IsCollisionEnabled = false;
                //carriage.InvisibleVehicle.Delete();
            }

            Vehicle locomotive = train;

            // Apply different forces to make crash look better

            var direction = Vector3.WorldUp;
            var rotation = new Vector3(0, 65, 0);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
                locomotive, 3,
                direction.X, direction.Y, direction.Z,
                rotation.X, rotation.Y, rotation.Z,
                locomotive.Bones["fwheel_1"].Index,
                false, true, true, false, true);

            direction = locomotive.RightVector;
            rotation = new Vector3(0, 100, 0);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
                locomotive, 5,
                direction.X, direction.Y, direction.Z,
                rotation.X, rotation.Y, rotation.Z,
                locomotive.Bones["fwheel_1"].Index,
                false, true, true, false, true);
            direction = locomotive.UpVector;
            rotation = new Vector3(0, 0, 0);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
                locomotive, 5,
                direction.X, direction.Y, direction.Z,
                rotation.X, rotation.Y, rotation.Z,
                locomotive.Bones["fwheel_1"].Index,
                false, true, true, false, true);

            IsDerailed = true;
        }

        /// <summary>
        /// Derails if train going is too fast on sharp corner
        /// </summary>
        private void ProcessSpeedDerail()
        {
            // We're basically comparing forward vector of previous frame and current frame
            // and if difference is too high and speed is higher than derailing minumum then train derails.
            var forwardVector = train.ForwardVector;

            if (physx.AbsoluteSpeed >= derailMinSpeed)
            {
                float angle = Vector3.Angle(forwardVector, prevForwardAngle);

                if (angle >= derailAngle)
                {
                    if (FusionUtils.Random.NextDouble() >= 0.3f)
                    {
                        Derail();
                    }
                }
            }
            prevForwardAngle = forwardVector;
        }
    }
}
