using System;
using Kturker.UnityBase.Features.Showcase.Common;
using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    // The runner's jump and star sounds; the scene's scope disposes them
    public sealed class RunnerSounds : IDisposable
    {
        public RunnerSounds(AudioClip jump, AudioClip star)
        {
            Jump = jump;
            Star = star;
        }

        public AudioClip Jump { get; }
        public AudioClip Star { get; }

        public static RunnerSounds Create() =>
            new RunnerSounds(ToneClip.Create("Jump", 660f, 0.08f), ToneClip.Create("Star", 990f, 0.12f));

        public void Dispose()
        {
            Release(Jump);
            Release(Star);
        }

        private static void Release(AudioClip clip)
        {
            if (clip == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(clip);
            else UnityEngine.Object.DestroyImmediate(clip);
        }
    }
}
