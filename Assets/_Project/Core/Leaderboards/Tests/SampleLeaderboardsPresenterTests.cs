using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Features.Sample.Leaderboards;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Leaderboards.Tests
{
    public sealed class SampleLeaderboardsPresenterTests
    {
        private FakeGameServices services;
        private FakeSampleView sample;
        private FakePreferences prefs;
        private FakeLeaderboardView view;
        private FakeUiFeedback feedback;

        [SetUp]
        public void Create()
        {
            services = new FakeGameServices();
            sample = new FakeSampleView();
            prefs = new FakePreferences();
            view = new FakeLeaderboardView();
            feedback = new FakeUiFeedback();
        }

        private SampleLeaderboardsPresenter Started()
        {
            var presenter = new SampleLeaderboardsPresenter(view, sample, services, prefs, feedback);
            presenter.Start();
            return presenter;
        }

        [Test]
        public void Complete_reports_the_score_the_first_achievement_and_a_step()
        {
            Started();

            sample.ClickComplete();

            Assert.That(services.Calls, Is.EqualTo(new[] { "score 1", "unlock firstCompletion", "increment tenCompletions 1" }));
        }

        [Test]
        public void The_second_Complete_reports_score_two_and_a_step_but_no_unlock()
        {
            Started();

            sample.ClickComplete();
            sample.ClickComplete();

            Assert.That(services.Calls.GetRange(3, 2), Is.EqualTo(new[] { "score 2", "increment tenCompletions 1" }));
            Assert.That(services.Calls.Count, Is.EqualTo(5));
        }

        [Test]
        public void The_completion_count_survives_a_new_scene()
        {
            var first = Started();
            sample.ClickComplete();
            first.Dispose();

            Started();
            sample.ClickComplete();

            Assert.That(services.Calls.GetRange(3, 2), Is.EqualTo(new[] { "score 2", "increment tenCompletions 1" }));
        }

        [Test]
        public void Unavailable_services_show_the_hint_and_open_nothing()
        {
            services.Available = false;
            Started();

            view.ClickLeaderboard();
            Assert.That(view.Hint, Is.EqualTo(LeaderboardsKeys.NotSetUp));
            view.HideHint();
            view.ClickAchievements();

            Assert.That(view.Hint, Is.EqualTo(LeaderboardsKeys.NotSetUp));
            Assert.That(services.Calls, Is.Empty);
        }

        [Test]
        public void The_buttons_open_the_board_and_the_achievements()
        {
            Started();

            view.ClickLeaderboard();
            view.ClickAchievements();

            Assert.That(services.Calls, Is.EqualTo(new[] { "showLeaderboard", "showAchievements" }));
            Assert.That(view.Hint, Is.Null);
            Assert.That(feedback.Clicks, Is.EqualTo(2));
        }

        [Test]
        public void Leaving_the_scene_stops_the_reports()
        {
            Started().Dispose();

            sample.ClickComplete();
            view.ClickLeaderboard();
            view.ClickAchievements();

            Assert.That(services.Calls, Is.Empty);
        }

        [Test]
        public void Reports_stop_after_the_scene_is_left_mid_flight()
        {
            var held = new UniTaskCompletionSource<GameServicesResult>();
            services.PendingScore = held;
            var presenter = Started();

            sample.ClickComplete();
            presenter.Dispose();
            held.TrySetResult(GameServicesResult.Ok);

            Assert.That(services.Calls, Is.EqualTo(new[] { "score 1" }));
        }
    }
}
