using UnityEngine;

namespace Kturker.UnityBase.Core.Audio
{
    // Two sources on the root prefab: looping music and one-shot SFX. Levels from AudioPreferences go straight to
    // AudioSource.volume (no mixer). AudioSources ignore Time.timeScale, so UI clicks still play while paused.
    public sealed class AudioManager : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] private AudioSource music;
        [SerializeField] private AudioSource sfx;
        private AudioPreferences preferences;

        public void Bind(AudioPreferences value)
        {
            if (preferences != null) preferences.Changed -= Apply;
            preferences = value;
            preferences.Changed += Apply;
            Apply();
        }

        private void OnDestroy()
        {
            if (preferences != null) preferences.Changed -= Apply;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            music.clip = clip;
            music.loop = true;
            music.Play();
        }

        public void StopMusic() => music.Stop();

        public void PauseMusic() => music.Pause();

        public void ResumeMusic() => music.UnPause();

        public void PlaySfx(AudioClip clip)
        {
            if (clip != null) sfx.PlayOneShot(clip);
        }

        private void Apply()
        {
            music.volume = preferences.EffectiveMusic;
            sfx.volume = preferences.EffectiveSfx;
        }
    }
}
