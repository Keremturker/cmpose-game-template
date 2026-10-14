using System.Collections.Generic;
using System.Linq;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Haptics;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Track3D.Tests
{
    public sealed class Track3DPresenterTests
    {
        private const float Frame = 0.01f;

        private FakeTrackWorldView world;
        private FakeShowcaseHudView hud;
        private FakeTrackInput input;
        private FakeAudioPlayer audio;
        private FakeVibrator vibrator;
        private FakeLog log;

        [SetUp]
        public void SetUp()
        {
            world = new FakeTrackWorldView();
            hud = new FakeShowcaseHudView();
            input = new FakeTrackInput();
            audio = new FakeAudioPlayer();
            vibrator = new FakeVibrator();
            log = new FakeLog();
        }

        // Nothing on the track for a long way
        private static TrackConfig Open() => new TrackConfig { FirstRowAt = 1e6f };

        // Only gaps from distance 4, no gems: without a jump the ball falls in about a second
        private static TrackConfig Gaps() => new TrackConfig { FirstRowAt = 4f, GapChance = 1f, GemChance = 0f };

        private Track3DPresenter Create(TrackConfig config, IPremiumStatus premium = null)
        {
            var presenter = new Track3DPresenter(world, hud, input, config, new TrackSounds(null, null, null), audio,
                new HapticsService(new FakePreferences(), vibrator), log, premium, 1);
            presenter.Start();
            return presenter;
        }

        private void Begin(Track3DPresenter presenter)
        {
            input.Push(TrackInput.Jump);
            presenter.Advance(Frame);
        }

        private static void PlayUntilCrash(Track3DPresenter presenter, float seconds = 10f)
        {
            for (var t = 0f; t < seconds && !presenter.IsCrashed; t += Frame) presenter.Advance(Frame);
        }

        [Test]
        public void Start_shows_the_hint_and_a_zero_score_draws_the_track_and_marks_the_feature()
        {
            Create(new TrackConfig());

            Assert.That((hud.HintVisible, hud.Score), Is.EqualTo((true, 0)));
            Assert.That(world.Renders, Is.EqualTo(1));
            Assert.That(world.Last.Rows, Is.Not.Empty);
            Assert.That(world.Premium, Is.False);
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_FEATURE track3d"));
        }

        [Test]
        public void The_first_input_starts_the_run_and_hides_the_hint()
        {
            var presenter = Create(Open());
            presenter.Advance(Frame);
            Assert.That((hud.HintVisible, world.Last.Phase), Is.EqualTo((true, TrackPhase.Ready)));

            Begin(presenter);
            for (var i = 0; i < 50; i++) presenter.Advance(Frame);

            Assert.That(hud.HintVisible, Is.False);
            Assert.That(hud.Score, Is.GreaterThan(0));
            Assert.That(world.Last.Phase, Is.EqualTo(TrackPhase.Running));
        }

        [Test]
        public void Swipes_reach_the_track_one_per_frame()
        {
            var presenter = Create(Open());
            Begin(presenter);

            input.Push(TrackInput.Left);
            presenter.Advance(Frame);
            Assert.That(world.Last.Lane, Is.EqualTo(0));

            input.Push(TrackInput.Right);
            input.Push(TrackInput.Right);
            presenter.Advance(Frame);
            Assert.That(world.Last.Lane, Is.EqualTo(1));
            presenter.Advance(Frame);
            Assert.That(world.Last.Lane, Is.EqualTo(2));
        }

        [Test]
        public void A_jump_plays_the_jump_sound()
        {
            var presenter = Create(Open());
            Begin(presenter);

            input.Push(TrackInput.Jump);
            presenter.Advance(Frame);

            Assert.That(audio.SfxPlayed, Is.EqualTo(1));
        }

        [Test]
        public void A_gem_plays_the_gem_sound()
        {
            var presenter = Create(new TrackConfig { FirstRowAt = 10f, GapChance = 0f, TwoBlockChance = 0f, GemChance = 1f, Acceleration = 0f });
            Begin(presenter);
            var sim = world.Last;
            var run = sim.Gems.Where(g => g.Z < sim.Rows[1].Start).ToList();
            Assert.That(run, Is.Not.Empty);

            while (sim.Lane != run[0].Lane)
            {
                input.Push(sim.Lane < run[0].Lane ? TrackInput.Right : TrackInput.Left);
                presenter.Advance(Frame);
            }
            while (sim.Distance < run[run.Count - 1].Z + 1f && !presenter.IsCrashed) presenter.Advance(Frame);

            Assert.That(presenter.IsCrashed, Is.False);
            Assert.That(sim.Collected, Is.EqualTo(run.Count));
            Assert.That(audio.SfxPlayed, Is.EqualTo(run.Count));
        }

        [Test]
        public void A_fall_plays_the_crash_sound_vibrates_heavy_and_raises_Crashed_once_with_the_result()
        {
            var presenter = Create(Gaps());
            var crashes = new List<RunResult>();
            presenter.Crashed += crashes.Add;
            Begin(presenter);

            PlayUntilCrash(presenter);
            presenter.Advance(Frame);
            presenter.Advance(Frame);

            Assert.That(crashes.Count, Is.EqualTo(1));
            Assert.That(crashes[0].Score, Is.EqualTo(hud.Score));
            Assert.That(crashes[0].Stars, Is.EqualTo(world.Last.Collected));
            Assert.That((presenter.IsCrashed, presenter.CanContinue), Is.EqualTo((true, true)));
            Assert.That(audio.SfxPlayed, Is.EqualTo(1));
            Assert.That(vibrator.Played, Does.Contain(HapticStrength.Heavy));
            Assert.That(log.Lines, Does.Contain($"MARKER CMPOSE_RUN crashed score={crashes[0].Score}"));
        }

        [Test]
        public void Continue_works_once_and_Finish_then_Restart_start_a_new_run()
        {
            var presenter = Create(Gaps());
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
            Assert.That(world.Last.Phase, Is.EqualTo(TrackPhase.Ready));
        }

        [Test]
        public void Finish_and_Restart_out_of_turn_do_nothing()
        {
            var presenter = Create(Gaps());
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
        public void Input_on_the_game_over_screen_does_not_start_the_next_run()
        {
            var presenter = Create(Gaps());
            Begin(presenter);
            PlayUntilCrash(presenter);

            input.Push(TrackInput.Jump);
            presenter.Advance(Frame);
            presenter.Finish();
            presenter.Restart();
            presenter.Advance(Frame);

            Assert.That(world.Last.Phase, Is.EqualTo(TrackPhase.Ready));
            Assert.That(hud.HintVisible, Is.True);
        }

        [Test]
        public void A_paused_frame_moves_nothing_and_drops_the_input()
        {
            var presenter = Create(Open());
            // Paused before the first touch: the tap does not start the run, not even once the pause ends
            input.Push(TrackInput.Jump);
            presenter.Advance(0f);
            presenter.Advance(Frame);
            Assert.That((world.Last.Phase, hud.HintVisible), Is.EqualTo((TrackPhase.Ready, true)));

            Begin(presenter);
            for (var i = 0; i < 10; i++) presenter.Advance(Frame);
            var distance = world.Last.Distance;

            input.Push(TrackInput.Left);
            presenter.Advance(0f);
            Assert.That((world.Last.Lane, world.Last.Distance), Is.EqualTo((1, distance)));

            presenter.Advance(Frame);
            Assert.That(world.Last.Lane, Is.EqualTo(1));
            Assert.That(world.Last.Distance, Is.GreaterThan(distance));
        }

        [Test]
        public void Premium_paints_the_ball_gold_and_follows_changes_until_disposed()
        {
            var premium = new FakePremiumStatus { IsPremium = true };
            var presenter = Create(Gaps(), premium);
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
