using System;
using Kturker.UnityBase.Core.Audio;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Haptics;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Features.Showcase.Runner2D.Domain;
using UnityEngine;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    // Drives the simulation every frame with the scaled delta time (the pause stops it), draws it, plays the jump and
    // star sounds and the crash haptic, and offers the run to the shared parts (IShowcaseRun). A registered
    // IPremiumStatus (the purchases flag) paints the runner gold.
    public sealed class Runner2DPresenter : IStartable, ITickable, IDisposable, IShowcaseRun
    {
        private readonly IRunnerWorldView world;
        private readonly IShowcaseHudView hud;
        private readonly ITapInput input;
        private readonly RunnerConfig config;
        private readonly RunnerSounds sounds;
        private readonly IAudioPlayer audio;
        private readonly HapticsService haptics;
        private readonly ILog log;
        private readonly IPremiumStatus premium;
        private readonly int seed;
        private RunnerSimulation sim;
        private int runs;

        public Runner2DPresenter(IRunnerWorldView world, IShowcaseHudView hud, ITapInput input, RunnerConfig config,
            RunnerSounds sounds, IAudioPlayer audio, HapticsService haptics, ILog log, IPremiumStatus premium, int seed)
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

        public bool IsCrashed => sim.Phase == RunnerPhase.Crashed;
        public bool CanContinue => IsCrashed && !sim.Continued;

        public void Start()
        {
            if (premium != null) premium.Changed += Paint;
            Paint();
            NewRun();
            log.Marker("CMPOSE_FEATURE runner2d");
        }

        public void Dispose()
        {
            if (premium != null) premium.Changed -= Paint;
        }

        public void Tick() => Advance(Time.deltaTime);

        public void Advance(float seconds)
        {
            // A paused frame takes the tap and drops it, so it neither starts the run nor plays out after the pause
            var tapped = input.ConsumePress();
            var events = sim.Step(seconds, seconds > 0f && tapped, input.Held);
            if ((events & StepEvents.Started) != 0) hud.ShowHint(false);
            if ((events & StepEvents.Jumped) != 0) audio.PlaySfx(sounds.Jump);
            if ((events & StepEvents.StarCollected) != 0) audio.PlaySfx(sounds.Star);
            world.Render(sim);
            hud.ShowScore(sim.Score);
            if ((events & StepEvents.Crashed) == 0) return;
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
            if (sim.Phase == RunnerPhase.Over) NewRun();
        }

        private void NewRun()
        {
            sim = new RunnerSimulation(config, seed + runs++);
            hud.ShowHint(true);
            hud.ShowScore(0);
            world.Render(sim);
        }

        private RunResult Result() => new RunResult(sim.Score, (int)sim.Distance, sim.Collected);

        private void Paint() => world.SetPremium(premium != null && premium.IsPremium);
    }
}
