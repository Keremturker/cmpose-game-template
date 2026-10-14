using Kturker.UnityBase.Core.Leaderboards;
using Kturker.UnityBase.Core.Leaderboards.Tests;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Common.Leaderboards.Tests
{
    public sealed class ShowcaseScoresPresenterTests
    {
        private FakeShowcaseScoresView view;
        private FakeGameServices services;
        private FakeUiFeedback feedback;

        [SetUp]
        public void SetUp()
        {
            view = new FakeShowcaseScoresView();
            services = new FakeGameServices();
            feedback = new FakeUiFeedback();
            new ShowcaseScoresPresenter(view, services, feedback).Start();
        }

        [Test]
        public void Unavailable_services_show_the_hint_and_open_nothing()
        {
            services.Available = false;

            view.ClickLeaderboard();

            Assert.That(view.Hint, Is.EqualTo(LeaderboardsKeys.NotSetUp));
            Assert.That(services.Calls, Is.Empty);
            Assert.That(feedback.Clicks, Is.EqualTo(1));
        }

        [Test]
        public void Leaderboard_opens_the_board()
        {
            view.ClickLeaderboard();

            Assert.That(services.Calls, Is.EqualTo(new[] { "showLeaderboard" }));
            Assert.That(view.HintsHidden, Is.EqualTo(1));
        }

        [Test]
        public void Achievements_open_the_achievements()
        {
            view.ClickAchievements();

            Assert.That(services.Calls, Is.EqualTo(new[] { "showAchievements" }));
        }
    }
}
