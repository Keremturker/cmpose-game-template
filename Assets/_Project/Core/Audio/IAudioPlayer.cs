using UnityEngine;

namespace Kturker.UnityBase.Core.Audio
{
    public interface IAudioPlayer
    {
        void PlayMusic(AudioClip clip);
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        void PlaySfx(AudioClip clip);
    }
}
