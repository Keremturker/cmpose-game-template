using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Common.Tests
{
    public sealed class GameOverPresenterTests
    {
        private FakeShowcaseRun run;
        private FakeGameOverView view;
        private FakePreferences preferences;
        private FakeSceneLoader loader;
        private FakeUiFeedback feedback;
        private GameOverPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            run = new FakeShowcaseRun();
            view = new FakeGameOverView();
            preferences = new FakePreferences();
            loader = new FakeSceneLoader();
            feedback = new FakeUiFeedback();
            presenter = new GameOverPresenter(view, run, new BestScore(preferences), loader, feedback);
            presenter.Start();
        }

        [Test]
        public void A_crash_shows_the_score_and_the_stored_best()
        {
            preferences.SetInt(BestScore.Key, 50);

            run.Crash(new RunResult(30, 25, 1));

            Assert.That(view.Visible, Is.True);
            Assert.That((view.Score, view.Best, view.NewBest), Is.EqualTo((30, 50, false)));
        }

        [Test]
        public void A_crash_above_the_best_shows_a_new_best_but_stores_it_only_when_the_run_ends()
        {
            preferences.SetInt(BestScore.Key, 10);

            run.Crash(new RunResult(30, 30, 0));

            Assert.That((view.Score, view.Best, view.NewBest), Is.EqualTo((30, 30, true)));
            Assert.That(preferences.GetInt(BestScore.Key, 0), Is.EqualTo(10));
        }

        [Test]
        public void Continuing_hides_the_panel()
        {
            run.Crash(new RunResult(30, 30, 0));

            run.Continue();

            Assert.That(view.Visible, Is.False);
        }

        [Test]
        public void Retry_ends_the_run_stores_the_best_and_starts_a_new_one()
        {
            run.Crash(new RunResult(30, 30, 0));

            view.ClickRetry();

            Assert.That((run.Finishes, run.Restarts), Is.EqualTo((1, 1)));
            Assert.That(view.Visible, Is.False);
            Assert.That(preferences.GetInt(BestScore.Key, 0), Is.EqualTo(30));
            Assert.That(feedback.Clicks, Is.EqualTo(1));
            Assert.That(loader.Requested, Is.Empty);
        }

        [Test]
        public void Menu_ends_the_run_and_opens_the_menu()
        {
            run.Crash(new RunResult(30, 30, 0));

            view.ClickMenu();

            Assert.That((run.Finishes, run.Restarts), Is.EqualTo((1, 0)));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
            Assert.That(preferences.GetInt(BestScore.Key, 0), Is.EqualTo(30));
        }

        [Test]
        public void Retry_and_Menu_do_nothing_while_the_run_is_not_crashed()
        {
            view.ClickRetry();
            view.ClickMenu();
            run.Crash(new RunResult(30, 30, 0));
            view.ClickRetry();
            view.ClickRetry();

            Assert.That((run.Finishes, run.Restarts), Is.EqualTo((1, 1)));
            Assert.That(loader.Requested, Is.Empty);
            Assert.That(feedback.Clicks, Is.EqualTo(1));
        }

        [Test]
        public void After_dispose_nothing_reaches_the_view()
        {
            presenter.Dispose();

            run.Crash(new RunResult(30, 30, 0));
            view.ClickMenu();

            Assert.That(view.Visible, Is.False);
            Assert.That(loader.Requested, Is.Empty);
        }
    }
}
