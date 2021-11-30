using AdvancedTrainSystem.Core.Components.Abstract;
using FusionLibrary.Extensions;
using FusionLibrary.Other;
using GTA;
using RageAudio;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    public class SteamSoundsComponent : SoundComponent
    {
        // FMOD Events

        private AudioEvent chugsEvent;
        private AudioEvent hissEvent;
        private AudioEvent wheelSlipEvent;
        private AudioEvent moveEvent;

        // FMOD Params

        private const string speedParam = "Speed";
        private const string safetyValveParam = "SafetyValve";
        private const string frictionParam = "Friction";
        private const string slipParam = "Slip";

        private SafetyValveComponent safetyValve;

        /// <summary>
        /// This dictionary contains delay values corresponding to its speed parameter of the chug sound.
        /// </summary>
        private readonly InterpolationDictionary delayToSpeed = new InterpolationDictionary()
        {
            // Don't change if chug sound bank wasn't modified!

            [10000] = 0.00f,
            [1000] = 0.11f,
            [700] = 0.16f,
            [500] = 0.24f,
            [333] = 0.39f,
            [250] = 0.58f,
            [200] = 0.64f,
            [166] = 0.70f,
            [125] = 0.83f,
            [100] = 1.00f
        };
        private float previousChugDelay = -1;
        private int lastChugPlayTime = 0;

        public SteamSoundsComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            safetyValve = Components.GetComponent<SafetyValveComponent>();

            chugsEvent = mainAudioSource.CreateEvent("event:/Ambient/Chug", true);
            hissEvent = mainAudioSource.CreateEvent("event:/Ambient/Hiss", true);
            wheelSlipEvent = mainAudioSource.CreateEvent("event:/Ambient/Slip", true);
            moveEvent = mainAudioSource.CreateEvent("event:/Ambient/Move", true);
        }

        /// <summary>
        /// Update all FMOD events.
        /// </summary>
        public override void Update()
        {
            ProcessChugsEvent();
            ProcessHissEvent();
            ProcessWheelSlip();
            ProcessMoveEvent();
        }

        private void ProcessMoveEvent()
        {
            // Added 0.01f cuz otherwise it refuses to play...
            moveEvent.SetParameter(speedParam, physx.AbsoluteSpeed.Remap(0, 20, 0, 1) + 0.01f);
        }

        private void ProcessChugsEvent()
        {
            // Calculate timer delay so chug sound plays 2 times per wheel rotation
            float wheelRotationsPerSecond = Math.Abs(physx.DriveWheelSpeed) / 6f; //TODO: Use actual drive wheel length
            //Parent.WheelComponent.DriveWheelLength;

            float chugDelay = Math.Max(500 / wheelRotationsPerSecond, 100);

            // Get speed value and update event parameter
            float speed = delayToSpeed.GetInterpolatedValue(chugDelay);
            chugsEvent.SetParameter(speedParam, speed);

            //GTA.UI.Screen.ShowSubtitle($"Chug Delay: {chugDelay:0} Speed: {speed:0.00}");

            // Timer made in order to properly work with dynamically changing delay
            if (Game.GameTime - lastChugPlayTime > chugDelay)
            {
                // Reset event timeline to play chug sound, could be considered as hack
                // but we don't know or there's just no other way to do it
                chugsEvent.EventInstance.setTimelinePosition(0);

                lastChugPlayTime = Game.GameTime;
            }
            previousChugDelay = chugDelay;
        }

        private void ProcessWheelSlip()
        {
            float speed = physx.AbsoluteSpeed.Remap(0, 15, 0, 1);
            float slip = physx.WheelSlip;

            //wheelSlipEvent.SetParameter(speedParam, speed);
            wheelSlipEvent.SetParameter(slipParam, slip);
        }

        private void ProcessHissEvent()
        {
            hissEvent.SetParameter(safetyValveParam, safetyValve.Valve);
        }
    }
}
