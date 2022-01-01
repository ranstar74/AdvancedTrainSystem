using RageAudio;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Controls train sounds.</summary>
    public class Sounds : TrainComponent
    {
        private AudioPlayer audioPlayer;

        /// <summary>Audio source that is located at 'Chassis' bone.</summary>
        protected AudioSource mainAudioSource;

        public Sounds(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            // Intialize audio player and load banks
            audioPlayer = new AudioPlayer();

            foreach(string bank in Train.TrainData.SoundBanks)
            {
                audioPlayer.LoadBank("scripts/ATS/Audio/" + bank);
            }

            // Create audio source from train and audio events
            mainAudioSource = audioPlayer.CreateAudioSource(Train);
        }

        public override void Dispose()
        {
            audioPlayer.Dispose();
        }
    }
}
