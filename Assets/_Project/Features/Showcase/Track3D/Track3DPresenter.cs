using System;
using Kturker.UnityBase.Core.Audio;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Haptics;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
using UnityEngine;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Showcase.Track3D
{
    // Drives the track every frame with the scaled delta time (the pause stops it and drops what was swiped meanwhile),
    // draws it, plays the jump, gem and crash sounds and the crash haptic, and offers the run to the shared parts
    // (IShowcaseRun; the gems count as the run's stars). A registered IPremiumStatus (the purchases flag) paints the
    // ball gold.
    public sealed class Track3DPresenter : IStartable, ITickable, IDisposable, IShowcaseRun
    {
        private readonly ITrackWorldView world;
        private readonly IShowcaseHudView hud;
        private readonly ITrackInput input;
        private readonly TrackConfig config;
        private readonly TrackSounds sounds;
        private readonly IAudioPlayer audio;
        private readonly HapticsService haptics;
        private readonly ILog log;
        private readonly IPremiumStatus premium;
        private readonly int seed;
        private TrackSimulation sim;
        private int runs;

        public Track3DPresenter(ITrackWorldView world, IShowcaseHudView hud, ITrackInput input, TrackConfig config,
            TrackSounds sounds, IAudioPlayer audio, HapticsService haptics, ILog log, IPremiumStatus premium, int seed)
        {
            this.world = world;
            this.hud = hud;
            this.input = input;
            this.config = config;
            this.sounds = sounds;
            this.audio = audio;
            this.haptics = haptics;
            this.log = log;
            this.premium = premium;
            this.seed = seed;
        }

        public event Action<RunResult> Crashed;
        public event Action Continued;
        public event Action<RunResult> Finished;

        public bool IsCrashed => sim.Phase == TrackPhase.Crashed;
        public bool CanContinue => IsCrashed && !sim.Continued;

        public void Start()
        {
            if (premium != null) premium.Changed += Paint;
            Paint();
            NewRun();
            log.Marker("CMPOSE_FEATURE track3d");
        }

        public void Dispose()
        {
            if (premium != null) premium.Changed -= Paint;
        }

        public void Tick() => Advance(Time.deltaTime);

        public void Advance(float seconds)
        {
            // A paused frame takes the input and drops it, so it neither starts the run nor plays out after the pause
            var swiped = input.Consume();
            var events = sim.Step(seconds, seconds > 0f ? swiped : TrackInput.None);
            if ((events & StepEvents.Started) != 0) hud.ShowHint(false);
            if ((events & StepEvents.Jumped) != 0) audio.PlaySfx(sounds.Jump);
            if ((events & StepEvents.GemCollected) != 0) audio.PlaySfx(sounds.Gem);
            world.Render(sim);
            hud.ShowScore(sim.Score);
            if ((events & StepEvents.Crashed) == 0) return;
            audio.PlaySfx(sounds.Crash);
            haptics.Play(HapticStrength.Heavy);
            log.Marker($"CMPOSE_RUN crashed score={sim.Score}");
            Crashed?.Invoke(Result());
        }

        public void Continue()
        {
            if (sim.Continue()) Continued?.Invoke();
        }

        public void Finish()
        {
            if (!sim.Finish()) return;
            log.Marker($"CMPOSE_RUN finished score={sim.Score}");
            Finished?.Invoke(Result());
        }

        public void Restart()
        {
            if (sim.Phase == TrackPhase.Over) NewRun();
        }

        private void NewRun()
        {
            sim = new TrackSimulation(config, seed + runs++);
            hud.ShowHint(true);
            hud.ShowScore(0);
            world.Render(sim);
        }

        private RunResult Result() => new RunResult(sim.Score, (int)sim.Distance, sim.Collected);

        private void Paint() => world.SetPremium(premium != null && premium.IsPremium);
    }
}
