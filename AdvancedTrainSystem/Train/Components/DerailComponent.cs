using AdvancedTrainSystem.Data;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using RageComponent;
using System;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Handles train derailnment.
    /// </summary>
    public class DerailComponent : Component<CustomTrain>
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
        /// Invokes on train derail.
        /// </summary>
        public Action OnDerail { get; set; }

        /// <summary>
        /// Whether train is derailed or not.
        /// </summary>
        public bool IsDerailed { get; private set; }

        /// <summary>
        /// Previous forward angle of train, used to derail on speed.
        /// </summary>
        private Vector3 _previousForwardAngle = Vector3.Zero;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Start()
        {
            Base.CollisionComponent.OnCollision += DerailOnCollision;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {
            ProcessSpeedDerail();
        }

        /// <summary>
        /// Invokes derail on collision.
        /// </summary>
        /// <param name="colInfo"></param>
        private void DerailOnCollision(CollisionInfo colInfo)
        {
            Derail();
        }

        /// <summary>
        /// Derails train with all carriages.
        /// </summary>
        public void Derail()
        {
            if (IsDerailed)
                return;

            OnDerail?.Invoke();

            // Process all carriages from locomotive to last one
            for (int i = 0; i < Base.Carriages.Count; i++)
            {
                var carriage = Base.Carriages[i];

                // Disable invisible vehicle collision first and then detach
                // visible model, otherwise they will collide with eachother
                carriage.InvisibleVehicle.IsCollisionEnabled = false;

                // TODO: Make player fly like out of cars in gta 4
                // Throw player out of train
                if (Game.Player.Character.IsInVehicle(carriage.InvisibleVehicle))
                {
                    Game.Player.Character.Task.LeaveVehicle(LeaveVehicleFlags.BailOut);
                    Game.Player.Character.Ragdoll(10, RagdollType.Normal);
                }

                // Attach carriage as trailer to next carriage if theres one
                if (carriage.Next != null)
                {
                    carriage.VisibleVehicle.AttachToTrailer(carriage.Next.VisibleVehicle, 130);
                }

                // Detach visible vehicle from invisible one and re-apply velocity
                carriage.VisibleVehicle.Detach();
                carriage.VisibleVehicle.Velocity = carriage.CustomTrain.TrainHead.Velocity;
            }

            var trainHead = Base.TrainHeadVisible;

            // Apply different forces to make crash look better

            var direction = Vector3.WorldUp;
            var rotation = new Vector3(0, 65, 0);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
                trainHead, 3,
                direction.X, direction.Y, direction.Z,
                rotation.X, rotation.Y, rotation.Z,
                trainHead.Bones["fwheel_1"].Index,
                false, true, true, false, true);

            direction = trainHead.RightVector;
            rotation = new Vector3(0, 100, 0);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
                trainHead, 5,
                direction.X, direction.Y, direction.Z,
                rotation.X, rotation.Y, rotation.Z,
                trainHead.Bones["fwheel_1"].Index,
                false, true, true, false, true);
            direction = trainHead.UpVector;
            rotation = new Vector3(0, 0, 0);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
                trainHead, 5,
                direction.X, direction.Y, direction.Z,
                rotation.X, rotation.Y, rotation.Z,
                trainHead.Bones["fwheel_1"].Index,
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
            var forwardVector = Entity.ForwardVector;
            if (Math.Abs(Base.SpeedComponent.Speed) >= DerailMinSpeed)
            {
                float angle = Vector3.Angle(forwardVector, _previousForwardAngle);

                if (angle >= DerailAngle)
                {
                    if (FusionUtils.Random.NextDouble() >= 0.3f)
                    {
                        Derail();
                    }
                }
            }
            _previousForwardAngle = forwardVector;
        }
    }
}
