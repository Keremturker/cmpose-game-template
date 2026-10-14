using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    // A short sine beep that fades out, made in code (the template ships no sound files). The showcase games make
    // their sounds with it.
    public static class ToneClip
    {
        private const int Rate = 44100;

        public static AudioClip Create(string name, float frequency, float seconds)
        {
            var samples = new float[(int)(Rate * seconds)];
            for (var i = 0; i < samples.Length; i++)
            {
                var fade = 1f - (float)i / samples.Length;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / Rate) * 0.3f * fade;
            }
            var clip = AudioClip.Create(name, samples.Length, 1, Rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
