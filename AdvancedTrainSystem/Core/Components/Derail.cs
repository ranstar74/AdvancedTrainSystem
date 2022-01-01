using FusionLibrary.Extensions;
using GTA;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Controls train derailnment.</summary>
    public class Derail : TrainComponent
    {
        /// <summary>Invokes on train derail.</summary>
        public Action OnDerail { get; set; }

        /// <summary>Whether train is derailed or not.</summary>
        public bool IsDerailed { get; private set; }

        private int _derailTime = -1;

        /// <summary>
        /// Creates a new instance of <see cref="Components.Derail"/>.
        /// </summary>
        /// <param name="components"></param>
        public Derail(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            //if (train.IsAtsDerailed())
            //    Derail();
        }

        public override void Update()
        {
            if (Math.Abs(Motion.Angle) > 30f)
            {
                DoDerail();
            }

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
                for (int i = 0; i < Train.Carriages.Count; i++)
                {
                    Vehicle carriage = Train.Carriages[i].Vehicle;

                    carriage.Velocity = carriage.ForwardVector * Physx.Speed;

                    // Attach carriage as trailer to next carriage if theres one
                    if (previousCarriage != null)
                    {
                        previousCarriage.AttachToTrailer(carriage, 360);
                    }

                    previousCarriage = carriage;
                }
            }
        }

        /// <summary>Derails train with all carriages.</summary>
        public void DoDerail()
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
            foreach(TrainCarriage carriage in Train.Carriages)
            {
                Vehicle vehicle = carriage.Vehicle;
                Vehicle hiddenVehicle = carriage.HiddenVehicle;

                hiddenVehicle.IsCollisionEnabled = false;

                vehicle.Detach();
                vehicle.IsCollisionEnabled = false;
            }
            OnDerail?.Invoke();

            Script.Yield();
            foreach(TrainCarriage carriage in Train.Carriages)
            {
                carriage.Vehicle.IsCollisionEnabled = true;
            }

            MarkAsDerailed();
        }

        private void MarkAsDerailed()
        {
            Train.ForEachCarriage(x =>
            {
                x.Decorator().SetBool(TrainConstants.IsDerailed, true);
            });

            IsDerailed = true;
        }
    }
}
