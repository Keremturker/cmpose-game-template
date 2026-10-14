using System;
using Kturker.UnityBase.Features.Showcase.Common;
using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Track3D
{
    // The track's jump, gem and crash sounds; the scene's scope disposes them
    public sealed class TrackSounds : IDisposable
    {
        public TrackSounds(AudioClip jump, AudioClip gem, AudioClip crash)
        {
            Jump = jump;
            Gem = gem;
            Crash = crash;
        }

        public AudioClip Jump { get; }
        public AudioClip Gem { get; }
        public AudioClip Crash { get; }

        public static TrackSounds Create() =>
            new TrackSounds(ToneClip.Create("Jump", 520f, 0.08f), ToneClip.Create("Gem", 1180f, 0.1f), ToneClip.Create("Crash", 150f, 0.25f));

        public void Dispose()
        {
            Release(Jump);
            Release(Gem);
            Release(Crash);
        }

        private static void Release(AudioClip clip)
        {
            if (clip == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(clip);
            else UnityEngine.Object.DestroyImmediate(clip);
        }
    }
}
