using System;
using Kturker.UnityBase.Core.Preferences;

namespace Kturker.UnityBase.Core.Audio
{
    // Music and SFX levels in [0, 1] plus mute, kept in preferences (named so because UnityEngine.AudioSettings
    // exists). Writes stay in memory until IPreferences.Save(); Changed fires only when a value really changes.
    public sealed class AudioPreferences
    {
        public const string MusicKey = "audio.music";
        public const string SfxKey = "audio.sfx";
        public const string MutedKey = "audio.muted";
        public const float DefaultMusic = 0.8f;
        public const float DefaultSfx = 1f;

        private readonly IPreferences preferences;

        public AudioPreferences(IPreferences preferences) => this.preferences = preferences;

        public event Action Changed;

        public float Music
        {
            get => Read(MusicKey, DefaultMusic);
            set => Write(MusicKey, DefaultMusic, value);
        }

        public float Sfx
        {
            get => Read(SfxKey, DefaultSfx);
            set => Write(SfxKey, DefaultSfx, value);
        }

        public bool Muted
        {
            get => preferences.GetBool(MutedKey, false);
            set
            {
                if (Muted == value) return;
                preferences.SetBool(MutedKey, value);
                Changed?.Invoke();
            }
        }

        public float EffectiveMusic => Muted ? 0f : Music;

        public float EffectiveSfx => Muted ? 0f : Sfx;

        private float Read(string key, float fallback) => Sanitize(preferences.GetFloat(key, fallback), fallback);

        private void Write(string key, float fallback, float value)
        {
            var current = Read(key, fallback);
            var next = Sanitize(value, current);
            if (next.Equals(current)) return;
            preferences.SetFloat(key, next);
            Changed?.Invoke();
        }

        // NaN keeps the fallback; anything else, infinities included, is clamped
        private static float Sanitize(float value, float fallback) =>
            float.IsNaN(value) ? fallback : Math.Min(1f, Math.Max(0f, value));
    }
}
