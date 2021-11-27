using AdvancedTrainSystem.Railroad;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
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
        private const float derailMinSpeed = 10;

        /// <summary>
        /// Minimum angle difference between current and previous frames to derail.
        /// </summary>
        private const float derailAngle = 0.01f;

        private int _derailTime = -1;
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

            // Don't change this value as it may cause issues
            // with derail angle.
            UpdateTime = 20;
        }

        public override void Update()
        {
            // TODO: Fix train still enterable after derail

            ProcessSpeedDerail();
            ProcessAttachTrailer();
        }

        /// <summary>
        /// Attaches carriages to each other
        /// so after derail they won't separate and
        /// fly in different directions
        /// </summary>
        private void ProcessAttachTrailer()
        {
            // Keep attaching trailer some time after derail
            // to make sure it attached
            if (IsDerailed && Game.GameTime - _derailTime < 250)
            {
                // Process all carriages from locomotive to last one
                Vehicle previousCarriage = null;
                for (int i = 0; i < train.Carriages.Count; i++)
                {
                    Vehicle carriage = train.Carriages[i].Vehicle;

                    // Attach carriage as trailer to next carriage if theres one
                    if (previousCarriage != null)
                    {
                        previousCarriage.AttachToTrailer(carriage, 360);
                    }

                    previousCarriage = carriage;
                }
            }
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
            for (int i = 0; i < train.Carriages.Count; i++)
            {
                Vehicle carriage = train.Carriages[i].Vehicle;
                Vehicle hiddenVehicle = train.Carriages[i].HiddenVehicle;

                // Detach visible vehicle from invisible one and re-apply velocity
                carriage.Detach();
                carriage.Velocity = hiddenVehicle.Velocity;

                // Delete invisible model as its not longer needed
                hiddenVehicle.IsCollisionEnabled = false;
            }

            //Vehicle locomotive = train;

            // Apply different forces to make crash look better

            //var direction = Vector3.WorldUp;
            //var rotation = new Vector3(0, 65, 0) * (physx.Speed / 50);
            //Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
            //    locomotive, 3,
            //    direction.X, direction.Y, direction.Z,
            //    rotation.X, rotation.Y, rotation.Z,
            //    locomotive.Bones["fwheel_1"].Index,
            //    false, true, true, false, true);

            //direction = locomotive.RightVector;
            //rotation = new Vector3(0, 100, 0);
            //Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
            //    locomotive, 5,
            //    direction.X, direction.Y, direction.Z,
            //    rotation.X, rotation.Y, rotation.Z,
            //    locomotive.Bones["fwheel_1"].Index,
            //    false, true, true, false, true);
            //direction = locomotive.UpVector;
            //rotation = new Vector3(0, 0, 0);
            //Function.Call(Hash.APPLY_FORCE_TO_ENTITY,
            //    locomotive, 5,
            //    direction.X, direction.Y, direction.Z,
            //    rotation.X, rotation.Y, rotation.Z,
            //    locomotive.Bones["fwheel_1"].Index,
            //    false, true, true, false, true);

            IsDerailed = true;
            _derailTime = Game.GameTime;
        }

        /// <summary>
        /// Derails if train going is too fast on sharp corner
        /// </summary>
        private void ProcessSpeedDerail()
        {
            // We're basically comparing forward vector of previous frame and current frame
            // and if difference is too high and speed is higher than
            // derailing minumum then train derails.
            Vector3 forwardVector = train.ForwardVector;

            if (physx.AbsoluteSpeed >= derailMinSpeed)
            {
                float angle = Vector3.Angle(forwardVector, prevForwardAngle) * Game.LastFrameTime;
                
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
