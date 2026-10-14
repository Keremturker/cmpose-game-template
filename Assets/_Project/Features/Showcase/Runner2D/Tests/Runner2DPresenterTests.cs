using System.Collections.Generic;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Haptics;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Features.Showcase.Runner2D.Domain;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Runner2D.Tests
{
    public sealed class Runner2DPresenterTests
    {
        private const float Frame = 0.01f;

        private FakeRunnerWorldView world;
        private FakeShowcaseHudView hud;
        private FakeTapInput input;
        private FakeAudioPlayer audio;
        private FakeVibrator vibrator;
        private FakeLog log;

        [SetUp]
        public void SetUp()
        {
            world = new FakeRunnerWorldView();
            hud = new FakeShowcaseHudView();
            input = new FakeTapInput();
            audio = new FakeAudioPlayer();
            vibrator = new FakeVibrator();
            log = new FakeLog();
        }

        // Only low obstacles from distance 3, no stars: without a jump the runner crashes in about half a second
        private static RunnerConfig Lows() => new RunnerConfig { FirstObstacleAt = 3f, HighChance = 0f, StarChance = 0f };

        private Runner2DPresenter Create(RunnerConfig config, IPremiumStatus premium = null)
        {
            var presenter = new Runner2DPresenter(world, hud, input, config, new RunnerSounds(null, null), audio,
                new HapticsService(new FakePreferences(), vibrator), log, premium, 1);
            presenter.Start();
            return presenter;
        }

        private void Begin(Runner2DPresenter presenter)
        {
            input.Tap();
            presenter.Advance(Frame);
        }

        private static void PlayUntilCrash(Runner2DPresenter presenter, float seconds = 10f)
        {
            for (var t = 0f; t < seconds && !presenter.IsCrashed; t += Frame) presenter.Advance(Frame);
        }

        [Test]
        public void Start_shows_the_hint_and_a_zero_score_and_marks_the_feature()
        {
            Create(Lows());

            Assert.That((hud.HintVisible, hud.Score), Is.EqualTo((true, 0)));
            Assert.That(world.Renders, Is.EqualTo(1));
            Assert.That(world.Premium, Is.False);
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_FEATURE runner2d"));
        }

        [Test]
        public void The_first_touch_starts_the_run_and_hides_the_hint()
        {
            var presenter = Create(new RunnerConfig { FirstObstacleAt = 1e6f });
            presenter.Advance(Frame);
            Assert.That(hud.HintVisible, Is.True);

            Begin(presenter);
            for (var i = 0; i < 50; i++) presenter.Advance(Frame);

            Assert.That(hud.HintVisible, Is.False);
            Assert.That(hud.Score, Is.GreaterThan(0));
            Assert.That(world.Last.Phase, Is.EqualTo(RunnerPhase.Running));
        }

        [Test]
        public void A_paused_frame_drops_the_tap()
        {
            var presenter = Create(new RunnerConfig { FirstObstacleAt = 1e6f });
            // Paused before the first touch: the tap does not start the run, not even once the pause ends
            input.Tap();
            presenter.Advance(0f);
            presenter.Advance(Frame);
            Assert.That((world.Last.Phase, hud.HintVisible), Is.EqualTo((RunnerPhase.Ready, true)));

            // Paused mid-run: the tap is not a jump, not even once the pause ends
            Begin(presenter);
            input.Tap();
            presenter.Advance(0f);
            presenter.Advance(Frame);
            Assert.That(audio.SfxPlayed, Is.EqualTo(0));
        }

        [Test]
        public void A_jump_plays_the_jump_sound()
        {
            var presenter = Create(new RunnerConfig { FirstObstacleAt = 1e6f });
            Begin(presenter);

            input.Tap();
            presenter.Advance(Frame);

            Assert.That(audio.SfxPlayed, Is.EqualTo(1));
        }

        [Test]
        public void A_star_plays_the_star_sound()
        {
            var presenter = Create(new RunnerConfig { FirstObstacleAt = 5f, HighChance = 1f, StarChance = 1f, StarHighY = 0.3f });
            Begin(presenter);

            for (var i = 0; i < 400; i++) presenter.Advance(Frame);

            Assert.That(presenter.IsCrashed, Is.False);
            Assert.That(audio.SfxPlayed, Is.GreaterThanOrEqualTo(1));
            Assert.That(audio.SfxPlayed, Is.EqualTo(world.Last.Collected));
        }

        [Test]
        public void A_crash_vibrates_heavy_and_raises_Crashed_once_with_the_result()
        {
            var presenter = Create(Lows());
            var crashes = new List<RunResult>();
            presenter.Crashed += crashes.Add;
            Begin(presenter);

            PlayUntilCrash(presenter);
            presenter.Advance(Frame);
            presenter.Advance(Frame);

            Assert.That(crashes.Count, Is.EqualTo(1));
            Assert.That(crashes[0].Score, Is.EqualTo(hud.Score));
            Assert.That((presenter.IsCrashed, presenter.CanContinue), Is.EqualTo((true, true)));
            Assert.That(vibrator.Played, Does.Contain(HapticStrength.Heavy));
            Assert.That(log.Lines, Does.Contain($"MARKER CMPOSE_RUN crashed score={crashes[0].Score}"));
        }

        [Test]
        public void Continue_works_once_and_Finish_then_Restart_start_a_new_run()
        {
            var presenter = Create(Lows());
            var continued = 0;
            var finished = new List<RunResult>();
            presenter.Continued += () => continued++;
            presenter.Finished += finished.Add;
            Begin(presenter);
            PlayUntilCrash(presenter);

            presenter.Continue();
            Assert.That((continued, presenter.IsCrashed), Is.EqualTo((1, false)));
            PlayUntilCrash(presenter, 30f);
            Assert.That((presenter.IsCrashed, presenter.CanContinue), Is.EqualTo((true, false)));
            presenter.Continue();
            Assert.That(continued, Is.EqualTo(1));

            presenter.Finish();
            presenter.Finish();
            Assert.That(finished.Count, Is.EqualTo(1));
            Assert.That(finished[0].Score, Is.EqualTo(hud.Score));
            Assert.That(log.Lines, Does.Contain($"MARKER CMPOSE_RUN finished score={finished[0].Score}"));

            presenter.Restart();
            Assert.That((hud.HintVisible, hud.Score, presenter.IsCrashed), Is.EqualTo((true, 0, false)));
            Assert.That(world.Last.Phase, Is.EqualTo(RunnerPhase.Ready));
        }

        [Test]
        public void Finish_and_Restart_out_of_turn_do_nothing()
        {
            var presenter = Create(Lows());
            var finished = 0;
            presenter.Finished += _ => finished++;
            Begin(presenter);

            presenter.Finish();
            presenter.Restart();
            Assert.That((finished, hud.HintVisible), Is.EqualTo((0, false)));

            PlayUntilCrash(presenter);
            presenter.Restart();
            Assert.That(presenter.IsCrashed, Is.True);
            Assert.That(finished, Is.Zero);
        }

        [Test]
        public void A_touch_on_the_game_over_screen_does_not_start_the_next_run()
        {
            var presenter = Create(Lows());
            Begin(presenter);
            PlayUntilCrash(presenter);

            input.Tap();
            presenter.Advance(Frame);
            presenter.Finish();
            presenter.Restart();
            presenter.Advance(Frame);

            Assert.That(world.Last.Phase, Is.EqualTo(RunnerPhase.Ready));
            Assert.That(hud.HintVisible, Is.True);
        }

        [Test]
        public void Premium_paints_the_runner_gold_and_follows_changes()
        {
            var premium = new FakePremiumStatus { IsPremium = true };
            var presenter = Create(Lows(), premium);
            Assert.That(world.Premium, Is.True);

            premium.IsPremium = false;
            premium.Raise();
            Assert.That(world.Premium, Is.False);

            presenter.Dispose();
            premium.IsPremium = true;
            premium.Raise();
            Assert.That(world.Premium, Is.False);
        }
    }
}
