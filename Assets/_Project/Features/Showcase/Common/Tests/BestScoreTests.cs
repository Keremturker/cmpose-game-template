using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Common.Tests
{
    public sealed class BestScoreTests
    {
        [Test]
        public void It_starts_at_zero_and_a_broken_stored_value_reads_as_zero()
        {
            var preferences = new FakePreferences();
            Assert.That(new BestScore(preferences).Value, Is.Zero);
            preferences.SetInt(BestScore.Key, -5);
            Assert.That(new BestScore(preferences).Value, Is.Zero);
        }

        [Test]
        public void A_higher_score_is_stored_and_saved()
        {
            var preferences = new FakePreferences();
            var best = new BestScore(preferences);

            Assert.That(best.Submit(120), Is.True);

            Assert.That(best.Value, Is.EqualTo(120));
            Assert.That(preferences.GetInt(BestScore.Key, 0), Is.EqualTo(120));
            Assert.That(preferences.Saves, Is.EqualTo(1));
        }

        [Test]
        public void An_equal_or_lower_score_changes_nothing()
        {
            var preferences = new FakePreferences();
            var best = new BestScore(preferences);
            best.Submit(120);

            Assert.That(best.Submit(120), Is.False);
            Assert.That(best.Submit(40), Is.False);

            Assert.That(best.Value, Is.EqualTo(120));
            Assert.That(preferences.Saves, Is.EqualTo(1));
        }
    }
}
