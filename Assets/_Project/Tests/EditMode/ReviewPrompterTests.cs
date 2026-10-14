using System.Linq;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Review;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class ReviewPrompterTests
    {
        private FakePreferences prefs;
        private FakeReviewRequester requester;
        private FakeLog log;

        [SetUp]
        public void Create()
        {
            prefs = new FakePreferences();
            requester = new FakeReviewRequester();
            log = new FakeLog();
        }

        // A new prompter on the same store is what a restart looks like
        private ReviewPrompter Prompter(StorePlatform platform = StorePlatform.Android, string appStoreId = "") =>
            new ReviewPrompter(prefs, requester, new StoreIdentity(platform, "com.example.game", appStoreId), log);

        [Test]
        public void The_first_two_positive_events_do_not_ask()
        {
            var prompter = Prompter();

            Assert.That(prompter.RecordPositiveEvent(), Is.False);
            Assert.That(prompter.RecordPositiveEvent(), Is.False);
            Assert.That(requester.Reviews, Is.EqualTo(0));
            Assert.That(prefs.GetInt(ReviewPrompter.PositiveEventsKey, 0), Is.EqualTo(2));
        }

        [Test]
        public void The_third_positive_event_asks_once_and_marks_it()
        {
            var prompter = Prompter();
            prompter.RecordPositiveEvent();
            prompter.RecordPositiveEvent();

            Assert.That(prompter.RecordPositiveEvent(), Is.True);
            Assert.That(requester.Reviews, Is.EqualTo(1));
            Assert.That(log.Lines.Count(l => l == "MARKER CMPOSE_REVIEW requested"), Is.EqualTo(1));
            Assert.That(prefs.GetInt(ReviewPrompter.NextThresholdKey, 0), Is.EqualTo(13));
            Assert.That(prefs.Saves, Is.EqualTo(3));
        }

        [Test]
        public void It_asks_again_at_the_13th_and_23rd_event_and_never_between()
        {
            var prompter = Prompter();

            var asked = Enumerable.Range(1, 25).Where(_ => prompter.RecordPositiveEvent()).ToArray();

            Assert.That(asked, Is.EqualTo(new[] { 3, 13, 23 }));
            Assert.That(requester.Reviews, Is.EqualTo(3));
        }

        [Test]
        public void The_count_and_threshold_survive_a_restart()
        {
            Prompter().RecordPositiveEvent();
            Prompter().RecordPositiveEvent();

            Assert.That(Prompter().RecordPositiveEvent(), Is.True);
            for (var i = 0; i < 9; i++) Assert.That(Prompter().RecordPositiveEvent(), Is.False);
            Assert.That(Prompter().RecordPositiveEvent(), Is.True);
        }

        [Test]
        public void Corrupt_stored_counts_neither_crash_nor_flood()
        {
            prefs.SetInt(ReviewPrompter.PositiveEventsKey, -40);
            Assert.That(Prompter().RecordPositiveEvent(), Is.False);
            Assert.That(prefs.GetInt(ReviewPrompter.PositiveEventsKey, 0), Is.EqualTo(1));

            prefs.SetInt(ReviewPrompter.PositiveEventsKey, 2);
            prefs.SetInt(ReviewPrompter.NextThresholdKey, -7);
            Assert.That(Prompter().RecordPositiveEvent(), Is.True);
            Assert.That(prefs.GetInt(ReviewPrompter.NextThresholdKey, 0), Is.EqualTo(13));

            prefs.SetInt(ReviewPrompter.PositiveEventsKey, int.MaxValue - 1);
            prefs.SetInt(ReviewPrompter.NextThresholdKey, int.MaxValue - 1);
            Assert.That(Prompter().RecordPositiveEvent(), Is.True);
            Assert.That(prefs.GetInt(ReviewPrompter.NextThresholdKey, 0), Is.EqualTo(int.MaxValue));
            Assert.That(Prompter().RecordPositiveEvent(), Is.False);
            Assert.That(Prompter().RecordPositiveEvent(), Is.False);
            Assert.That(prefs.GetInt(ReviewPrompter.PositiveEventsKey, 0), Is.EqualTo(int.MaxValue));
            Assert.That(requester.Reviews, Is.EqualTo(2));
        }

        [Test]
        public void Android_store_page_is_the_play_store_app_then_the_web_page()
        {
            var prompter = Prompter();

            Assert.That(prompter.CanOpenStorePage, Is.True);
            prompter.OpenStorePage();

            Assert.That(requester.StorePages, Is.EqualTo(new[]
            {
                ("market://details?id=com.example.game", "https://play.google.com/store/apps/details?id=com.example.game")
            }));
        }

        [Test]
        public void Ios_store_page_is_the_write_review_page_and_a_blank_id_hides_it()
        {
            var hidden = Prompter(StorePlatform.Ios, "  ");
            Assert.That(hidden.CanOpenStorePage, Is.False);
            hidden.OpenStorePage();
            Assert.That(requester.StorePages, Is.Empty);
            Assert.That(log.Lines, Has.Some.StartsWith("WARNING No store page"));

            var shown = Prompter(StorePlatform.Ios, " 1234567890 ");
            Assert.That(shown.CanOpenStorePage, Is.True);
            shown.OpenStorePage();
            Assert.That(requester.StorePages, Is.EqualTo(new[]
            {
                ("itms-apps://apps.apple.com/app/id1234567890?action=write-review", (string)null)
            }));
        }
    }
}
