using AdvancedTrainSystem.Railroad.Components.SteamComponents;
using FusionLibrary.Extensions;
using FusionLibrary.Other;
using GTA;
using RageAudio;
using RageComponent;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Core.Components.Abstract
{
    public class SoundComponent : Component
    {
        // TODO: Make it compatible with any train type

        private AudioPlayer audioPlayer;

        // FMOD Audio Sources

        /// <summary>
        /// Audio source of "Chassis" bone.
        /// </summary>
        protected AudioSource mainAudioSource;

        protected readonly Train train;
        protected PhysxComponent physx;

        public SoundComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            physx = Components.GetComponent<PhysxComponent>();

            // Intialize audio player and load banks
            audioPlayer = new AudioPlayer();
            train.TrainInfo.SoundBanks.ForEach(bank =>
            {
                audioPlayer.LoadBank("scripts/ATS/Audio/" + bank);
            });

            // Create audio source from train and audio events
            mainAudioSource = audioPlayer.CreateAudioSource(train);
        }
    }
}
