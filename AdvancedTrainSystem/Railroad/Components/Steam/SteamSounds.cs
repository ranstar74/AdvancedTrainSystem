using AdvancedTrainSystem.Core.Components;
using FusionLibrary.Extensions;
using FusionLibrary.Other;
using GTA;
using RageAudio;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    /// <summary>Controls steam train sounds.</summary>
    public class SteamSounds : Sounds
    {
        // FMOD Events

        private AudioEvent _chugsEvent;
        private AudioEvent _hissEvent;
        private AudioEvent _wheelSlipEvent;
        private AudioEvent _moveEvent;

        // FMOD Params

        private const string _speedParam = "Speed";
        private const string _safetyValveParam = "SafetyValve";
        private const string _frictionParam = "Friction";
        private const string _slipParam = "Slip";

        private SafetyValve _safetyValve;

        /// <summary>This dictionary contains delay values corresponding to its speed parameter of the chug sound.</summary>
        private readonly InterpolationDictionary _delayToSpeed = new InterpolationDictionary()
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
        private float _prevChugDelay = -1;
        private int _lastChugPlayTime = 0;

        public SteamSounds(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            _safetyValve = Components.GetComponent<SafetyValve>();

            _chugsEvent = mainAudioSource.CreateEvent("event:/Ambient/Chug", true);
            _hissEvent = mainAudioSource.CreateEvent("event:/Ambient/Hiss", true);
            _wheelSlipEvent = mainAudioSource.CreateEvent("event:/Ambient/Slip", true);
            _moveEvent = mainAudioSource.CreateEvent("event:/Ambient/Move", true);
        }

        /// <summary>Update all FMOD events.</summary>
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
            _moveEvent.SetParameter(_speedParam, Physx.AbsoluteSpeed.Remap(0, 20, 0, 1) + 0.01f);
        }

        private void ProcessChugsEvent()
        {
            // Calculate timer delay so chug sound plays 2 times per wheel rotation
            float wheelRotationsPerSecond = Math.Abs(Physx.DriveWheelSpeed) / 6f; //TODO: Use actual drive wheel length

            float chugDelay = Math.Max(500 / wheelRotationsPerSecond, 100);

            // Get speed value and update event parameter
            float speed = _delayToSpeed.GetInterpolatedValue(chugDelay);
            _chugsEvent.SetParameter(_speedParam, speed);

            //GTA.UI.Screen.ShowSubtitle($"Chug Delay: {chugDelay:0} Speed: {speed:0.00}");

            // Timer made in order to properly work with dynamically changing delay
            if (Game.GameTime - _lastChugPlayTime > chugDelay)
            {
                // Reset event timeline to play chug sound, could be considered as hack
                // but we don't know or there's just no other way to do it
                _chugsEvent.EventInstance.setTimelinePosition(0);

                _lastChugPlayTime = Game.GameTime;
            }
            _prevChugDelay = chugDelay;
        }

        private void ProcessWheelSlip()
        {
            float slip = Physx.WheelSlip + Motion.Angle * 2;

            _wheelSlipEvent.SetParameter(_slipParam, slip);
        }

        private void ProcessHissEvent()
        {
            _hissEvent.SetParameter(_safetyValveParam, _safetyValve.Valve);
        }
    }
}
