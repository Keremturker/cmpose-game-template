using System;
using Kturker.UnityBase.Core.Update;

namespace Kturker.UnityBase.Core.Ads
{
    // When an interstitial may show: not before startDelay after launch (this object is made at launch), not within
    // gap of the last one, and only once 'every' moments have passed since the last one. A moment whose ad could not
    // show (not loaded) keeps the count, so the next moment tries again.
    public sealed class InterstitialPacer
    {
        private readonly IClock clock;
        private readonly long startDelayMillis;
        private readonly long gapMillis;
        private readonly int every;
        private readonly long start;
        private long? lastShown;
        private int triggers;

        public InterstitialPacer(IClock clock, long startDelayMillis, long gapMillis, int every)
        {
            this.clock = clock;
            this.startDelayMillis = startDelayMillis;
            this.gapMillis = gapMillis;
            this.every = Math.Max(1, every);
            start = clock.NowMillis;
        }

        // A moment where an interstitial could show (a finished level, a Complete press): true when it may
        public bool Trigger()
        {
            triggers++;
            var now = clock.NowMillis;
            if (triggers < every || now - start < startDelayMillis) return false;
            return !lastShown.HasValue || now - lastShown.Value >= gapMillis;
        }

        public void Shown()
        {
            lastShown = clock.NowMillis;
            triggers = 0;
        }
    }
}
