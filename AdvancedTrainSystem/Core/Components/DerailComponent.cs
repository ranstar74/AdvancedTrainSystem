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
        /// Gets locomotive angle on Y axis.
        /// </summary>
        public float Angle => _carriagePrevVecs[0].Rotation.Y;

        private readonly Train train;

        private PhysxComponent _physx;
        private CollisionComponent _collision;

        private int _derailTime = -1;

        private Vector3 _noise = default;
        private readonly Random rand = new Random();

        /// <summary>
        /// This class defines information for calculating
        /// and displaying train angle on turns.
        /// </summary>
        private class RotationInfo
        {
            public Carriage Carriage { get; set; }
            public Vector3 PrevForwardVector { get; set; }
            public Vector3 Rotation { get; set; }

            public RotationInfo(Carriage carriage)
            {
                Carriage = carriage;
                PrevForwardVector = carriage.Vehicle.ForwardVector;
            }
        }

        private readonly List<RotationInfo> _carriagePrevVecs  = new List<RotationInfo>();

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
            _collision = Components.GetComponent<CollisionComponent>();
            _physx = Components.GetComponent<PhysxComponent>();

            _collision.OnCollision += Derail;

            //if (train.IsAtsDerailed())
            //    Derail();

            train.Carriages.ForEach(carriage =>
            {
                _carriagePrevVecs.Add(new RotationInfo(carriage));
            });
        }

        public override void Update()
        {
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

                    carriage.Velocity = carriage.ForwardVector * _physx.Speed;

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

            _derailTime = Game.GameTime;

            // DO NOT CHANGE ORDER OF COLLISION ENABLED / DISABLED
            // Explanation: When train derails, we have to switch
            // driving train from invisible to visible one,
            // this is done by DrivingComponent on OnDerail event,
            // but there's problem that game camera will collide
            // with invisible model collision for one frame
            // and it will be pretty much noticable.
            // --- SOLUTION ---
            // So we first disable hidden vehicle collision,
            // in this moment theres no vehicle with collision, cuz
            // attached vehicle doesn't have collision either.
            // Then we detach vehicle and instantly disable its collision,
            // player is still in hidden model. After that there's
            // no collision for for one frame. If we don't skip
            // one frame game doesn't apply IsCollisionEnabled
            // and that makes camera flick. After player is moved,
            // we can enable collision back.
            // Yes, such a hack.
            foreach(Carriage carriage in train.Carriages)
            {
                Vehicle vehicle = carriage.Vehicle;
                Vehicle hiddenVehicle = carriage.HiddenVehicle;

                hiddenVehicle.IsCollisionEnabled = false;

                vehicle.Detach();
                vehicle.IsCollisionEnabled = false;
            }
            OnDerail?.Invoke();

            Script.Yield();
            foreach(Carriage carriage in train.Carriages)
            {
                carriage.Vehicle.IsCollisionEnabled = true;
            }

            MarkAsDerailed();
        }

        private void MarkAsDerailed()
        {
            train.ForEachCarriage(x =>
            {
                x.Decorator().SetBool(Constants.IsDerailed, true);
            });
            IsDerailed = true;
        }

        /// <summary>
        /// Derails if train going is too fast on sharp corner
        /// </summary>
        private void ProcessSpeedDerail()
        {
            if (IsDerailed)
                return;

            // Create noise that adds some life to train when it moves...
            Vector3 noise = new Vector3(
                (float)rand.NextDouble(),
                (float)rand.NextDouble(),
                (float)rand.NextDouble());
            noise *= 0.5f;

            // Same as above on frameAngle, higher speed = more noise
            float noiseAmplitude = _physx.AbsoluteSpeed / 15;
            noiseAmplitude = noiseAmplitude.Clamp(0f, 1.25f);
            
            noise *= noiseAmplitude;

            // Make noise more "shaky" when speed raises
            float noiseSpeed = _physx.AbsoluteSpeed / 5;
            noiseSpeed = noiseSpeed.Clamp(0f, 3f);

            _noise = Vector3.Lerp(_noise, noise, Game.LastFrameTime * noiseSpeed);

            // Explained below
            float speedFactor = Game.LastFrameTime * 10000 * _physx.AbsoluteSpeed / 70;
            for (int i = 0; i < _carriagePrevVecs.Count; i++)
            {
                RotationInfo rotInfo = _carriagePrevVecs[i];
                Carriage carriage = rotInfo.Carriage;

                Vector3 forwardVector = carriage.Vehicle.ForwardVector;

                // Find angle by difference of forward vectors of this and previous frame
                float frameAngle = Vector3.SignedAngle(forwardVector, rotInfo.PrevForwardVector, Vector3.WorldUp);

                // Since frameAngle is too low, we first multiply it on 10000 (just value i found work good)
                // Then we multiply it one (Speed / 70), so on 70 m/s we will get multiplier 
                //      Which basically will give higher angle on higher speed,
                //      lower value to get higher angle on lower speeds
                frameAngle *= speedFactor;

                // Make angle non linear
                frameAngle *= frameAngle;
                frameAngle /= 10;

                ApplyAngleOnCarriage(carriage, frameAngle, rotInfo);

                _carriagePrevVecs[i].PrevForwardVector = forwardVector;
            }
        }

        private void ApplyAngleOnCarriage(Carriage carriage, float angle, RotationInfo rotInfo)
        {
            if (float.IsNaN(angle))
                angle = 0f;

            Vector3 rotation = new Vector3(0, angle, 0);

            rotInfo.Rotation = Vector3.Lerp(rotInfo.Rotation, rotation, Game.LastFrameTime);
            
            if (Math.Abs(angle) > 30f)
            {
                Derail();
                return;
            }

            carriage.Vehicle.AttachTo(
                entity: carriage.HiddenVehicle,
                rotation: rotInfo.Rotation + _noise);
        }
    }
}
