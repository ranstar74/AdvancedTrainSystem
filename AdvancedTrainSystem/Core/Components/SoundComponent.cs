using AdvancedTrainSystem.Railroad.Components.SteamComponents;
using FusionLibrary.Other;
using GTA;
using RageComponent;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Core.Components
{
    public class SoundComponent : Component
    {
        // TODO: Make it compatible with any train type

        private readonly RageAudio.AudioPlayer audioPlayer;

        // FMOD Audio Sources

        private RageAudio.AudioSource mainAudioSource;

        // FMOD Events

        private RageAudio.AudioEvent chugsEvent;
        private RageAudio.AudioEvent hissEvent;
        private RageAudio.AudioEvent wheelSlipEvent;

        // FMOD Params

        private const string speedParam = "Speed";
        private const string safetyValveParam = "SafetyValve";
        private const string frictionParam = "Friction";
        private const string slipParam = "Slip";

        private readonly Train train;
        private PhysxComponent physx;
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

        public SoundComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();

            // Intialize audio player and load banks
            audioPlayer = new RageAudio.AudioPlayer();
            train.TrainInfo.SoundBanks.ForEach(bank =>
            {
                audioPlayer.LoadBank(bank);
            });

            // Create audio source from train and audio events
            mainAudioSource = audioPlayer.CreateAudioSource(train);

            chugsEvent = mainAudioSource.CreateEvent("event:/Ambient/Chug", true);
            hissEvent = mainAudioSource.CreateEvent("event:/Ambient/Hiss", true);
            wheelSlipEvent = mainAudioSource.CreateEvent("event:/Ambient/Slip", true);
        }

        public override void Start()
        {
            physx = Components.GetComponent<PhysxComponent>();
        }

        /// <summary>
        /// Update all FMOD events.
        /// </summary>
        public override void Update()
        {
            ProcessChugsEvent();
            ProcessHissEvent();
            ProcessWheelSlip();
        }

        private float previousChugDelay = -1;
        private int lastChugPlayTime = 0;
        private void ProcessChugsEvent()
        {
            // Calculate timer delay so chug sound plays 2 times per wheel rotation
            float wheelRotationsPerSecond = Math.Abs(physx.DriveWheelSpeed) / 2.4f; //TODO: Use actual drive wheel length
            //Parent.WheelComponent.DriveWheelLength;

            float chugDelay = 500 / wheelRotationsPerSecond;

            // Don't process if delay hasn't changed
            if (chugDelay == previousChugDelay)
                return;

            // Get speed value and update event parameter
            float speed = delayToSpeed.GetInterpolatedValue((int)chugDelay);
            chugsEvent.SetParameter("speed", speed);

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
            //float speed = Parent.CustomTrain.SpeedComponent.AbsoluteSpeed.Remap(0, 15, 0, 1);
            float slip = physx.WheelSlipFactor;

            //GTA.UI.Screen.ShowSubtitle($"Slip: {slip:0.00}");

            //wheelSlipEvent.SetParameter(speedParam, speed);
            wheelSlipEvent.SetParameter(slipParam, slip);
        }

        private void ProcessHissEvent()
        {
            hissEvent.SetParameter(safetyValveParam, safetyValve.Valve);
        }
    }
}
