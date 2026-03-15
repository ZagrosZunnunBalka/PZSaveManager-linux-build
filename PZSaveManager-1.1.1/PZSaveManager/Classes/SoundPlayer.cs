using System;
using System.IO;
using ManagedBass;

namespace PZSaveManager.Classes
{
    public class SoundPlayer : IDisposable
    {
        public const int MaxVolume = 100;

        public static readonly SoundPlayer Shared = new();

        public bool IsPlaybackAllowed { get; set; } = true;

        private bool initialized = false;

        public SoundPlayer()
        {
            if (!Bass.Init())
                throw new Exception("BASS initialization failed");

            initialized = true;
        }

        public void PlaySaveEffect(SoundEffect effect)
        {
            Play(effect);
        }

        public void Play(SoundEffect effect)
        {
            if (!IsPlaybackAllowed)
                return;

            string path = effect switch
            {
                SoundEffect.Saving => "Resources/Audio/Saving.wav",
                SoundEffect.AlreadySaving => "Resources/Audio/AlreadySaving.wav",
                SoundEffect.SaveCanceled => "Resources/Audio/SaveCanceled.wav",
                SoundEffect.SaveNotCanceled => "Resources/Audio/SaveNotCanceled.wav",
                SoundEffect.SaveComplete => "Resources/Audio/SaveComplete.wav",
                SoundEffect.SaveFailure => "Resources/Audio/SaveFailure.wav",
                _ => throw new ArgumentOutOfRangeException(nameof(effect))
            };

            if (!File.Exists(path))
                return;

            int stream = Bass.CreateStream(path, 0, 0, BassFlags.AutoFree);

            if (stream != 0)
                Bass.ChannelPlay(stream);
        }

        public void Stop()
        {
            Bass.Stop();
        }

        public void Dispose()
        {
            if (initialized)
            {
                Bass.Free();
                initialized = false;
            }
        }
    }
}
