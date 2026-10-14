using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Review;
using Kturker.UnityBase.Features.Sample.Domain;
using Kturker.UnityBase.Features.Sample.Presentation;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class SamplePresenterTests
    {
        private static ReviewPrompter Review(FakeReviewRequester requester, FakeLog log) =>
            new ReviewPrompter(new FakePreferences(), requester, new StoreIdentity(StorePlatform.Android, "com.example.game", ""), log);

        [Test]
        public void Keys_carry_the_module_prefix() =>
            Assert.That(new[] { SampleKeys.Greeting, SampleKeys.Done, SampleKeys.Completed }, Is.All.StartsWith("sample."));

        [Test]
        public void Start_shows_the_greeting_and_marks_the_feature()
        {
            var view = new FakeSampleView { Label = "Shop" };
            var log = new FakeLog();
            new SamplePresenter(view, new FakeSceneLoader(), new FakeUiFeedback(), Review(new FakeReviewRequester(), log), new NoCompletionCounter(), log).Start();

            Assert.That(view.Greeting, Is.EqualTo("Shop"));
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_FEATURE Shop"));
        }

        [Test]
        public void Back_opens_the_menu_until_disposed()
        {
            var view = new FakeSampleView();
            var loader = new FakeSceneLoader();
            var feedback = new FakeUiFeedback();
            var log = new FakeLog();
            var presenter = new SamplePresenter(view, loader, feedback, Review(new FakeReviewRequester(), log), new NoCompletionCounter(), log);
            presenter.Start();

            view.ClickBack();
            presenter.Dispose();
            view.ClickBack();
            view.ClickComplete();

            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
            Assert.That(feedback.Clicks, Is.EqualTo(1));
        }

        [Test]
        public void Complete_says_well_done_and_the_third_one_asks_for_a_review()
        {
            var view = new FakeSampleView();
            var requester = new FakeReviewRequester();
            var feedback = new FakeUiFeedback();
            var log = new FakeLog();
            new SamplePresenter(view, new FakeSceneLoader(), feedback, Review(requester, log), new NoCompletionCounter(), log).Start();

            view.ClickComplete();
            Assert.That(view.DoneShown, Is.EqualTo(1));
            Assert.That(requester.Reviews, Is.EqualTo(0));
            view.ClickComplete();
            view.ClickComplete();

            Assert.That(requester.Reviews, Is.EqualTo(1));
            Assert.That(feedback.Clicks, Is.EqualTo(3));
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_REVIEW requested"));
        }

        [Test]
        public void Without_a_saved_count_no_count_is_shown()
        {
            var view = new FakeSampleView();
            var log = new FakeLog();
            new SamplePresenter(view, new FakeSceneLoader(), new FakeUiFeedback(), Review(new FakeReviewRequester(), log), new NoCompletionCounter(), log).Start();

            view.ClickComplete();
            Assert.That(view.Completed, Is.Null);
        }

        [Test]
        public void A_counter_shows_the_count_on_start_and_after_each_complete()
        {
            var view = new FakeSampleView();
            var log = new FakeLog();
            var counter = new FakeCompletionCounter { Count = 4 };
            new SamplePresenter(view, new FakeSceneLoader(), new FakeUiFeedback(), Review(new FakeReviewRequester(), log), counter, log).Start();

            Assert.That(view.Completed, Is.EqualTo(4));
            view.ClickComplete();
            Assert.That(view.Completed, Is.EqualTo(5));
        }
    }
}
