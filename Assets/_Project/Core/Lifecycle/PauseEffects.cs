using System;
using Kturker.UnityBase.Core.Audio;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Lifecycle
{
    // While paused: Time.timeScale = 0 and music paused; on resume the previous scale comes back.
    // timeScale 0 stops scaled time only: uGUI, scene loading and AudioSources keep working, and any future
    // UniTask.Delay in UI or loading code must use DelayType.Realtime (or UnscaledDeltaTime).
    public sealed class PauseEffects : IStartable, IDisposable
    {
        private readonly PauseService pause;
        private readonly ITimeScale time;
        private readonly IAudioPlayer audio;
        private float resumeScale = 1f;

        public PauseEffects(PauseService pause, ITimeScale time, IAudioPlayer audio)
        {
            this.pause = pause;
            this.time = time;
            this.audio = audio;
        }

        public void Start()
        {
            pause.Paused += OnPaused;
            pause.Resumed += OnResumed;
        }

        public void Dispose()
        {
            pause.Paused -= OnPaused;
            pause.Resumed -= OnResumed;
        }

        private void OnPaused()
        {
            resumeScale = time.Scale;
            time.Scale = 0f;
            audio.PauseMusic();
        }

        private void OnResumed()
        {
            time.Scale = resumeScale;
            audio.ResumeMusic();
        }
    }
}
