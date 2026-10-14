using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Ads.Tests
{
    public sealed class InterstitialPacerTests
    {
        [Test]
        public void Nothing_in_the_first_minute_after_launch()
        {
            var clock = new FakeClock { NowMillis = 1000 };
            var pacer = new InterstitialPacer(clock, 60000, 60000, 1);

            clock.NowMillis = 60999;
            Assert.That(pacer.Trigger(), Is.False);
            clock.NowMillis = 61000;
            Assert.That(pacer.Trigger(), Is.True);
        }

        [Test]
        public void Nothing_within_a_minute_of_the_last_interstitial()
        {
            var clock = new FakeClock();
            var pacer = new InterstitialPacer(clock, 0, 60000, 1);

            Assert.That(pacer.Trigger(), Is.True);
            pacer.Shown();
            clock.NowMillis = 59999;
            Assert.That(pacer.Trigger(), Is.False);
            clock.NowMillis = 60000;
            Assert.That(pacer.Trigger(), Is.True);
        }

        [Test]
        public void Every_second_moment_after_the_last_interstitial()
        {
            var pacer = new InterstitialPacer(new FakeClock(), 0, 0, 2);

            Assert.That(new[] { pacer.Trigger(), pacer.Trigger() }, Is.EqualTo(new[] { false, true }));
            pacer.Shown();
            Assert.That(new[] { pacer.Trigger(), pacer.Trigger() }, Is.EqualTo(new[] { false, true }));
        }

        [Test]
        public void A_moment_that_could_not_show_an_ad_does_not_reset_the_count()
        {
            var pacer = new InterstitialPacer(new FakeClock(), 0, 0, 2);

            pacer.Trigger();
            Assert.That(pacer.Trigger(), Is.True);
            Assert.That(pacer.Trigger(), Is.True);
        }
    }
}
