using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Ads.Tests
{
    public sealed class GmaAdsTests
    {
        private static readonly AdUnits Units = new AdUnits("banner-unit", "interstitial-unit", "rewarded-unit");

        private FakeGmaGateway gateway;
        private FakePremium premium;
        private FakeLog log;
        private List<int> waits;

        [SetUp]
        public void Create()
        {
            gateway = new FakeGmaGateway();
            premium = new FakePremium();
            log = new FakeLog();
            waits = new List<int>();
        }

        private GmaAds Make(int every = 1) =>
            new GmaAds(gateway, premium, new InterstitialPacer(new FakeClock(), 0, 0, every), Units, log, (seconds, _) =>
            {
                waits.Add(seconds);
                return UniTask.CompletedTask;
            });

        private static T Run<T>(UniTask<T> task) => task.GetAwaiter().GetResult();

        private IEnumerable<string> Markers => log.Lines.Where(l => l.StartsWith("MARKER CMPOSE_AD "));

        [Test]
        public void Unavailable_ads_never_touch_the_sdk()
        {
            var ads = Make();
            ads.ShowBanner();

            Assert.That(ads.IsAvailable, Is.False);
            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.NotReady));
            Assert.That(Run(ads.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.NotReady));
            Assert.That(gateway.InterstitialLoads + gateway.RewardedLoads + gateway.InterstitialShows + gateway.RewardedShows, Is.Zero);
            Assert.That(gateway.Banners, Is.Empty);
            Assert.That(Markers, Is.EqualTo(new[]
            {
                "MARKER CMPOSE_AD format=interstitial result=NotReady",
                "MARKER CMPOSE_AD format=rewarded result=NotReady"
            }));
        }

        [Test]
        public void Activation_preloads_both_formats_and_shows_a_banner_asked_for_earlier()
        {
            var ads = Make();
            var changes = 0;
            ads.Changed += () => changes++;
            ads.ShowBanner();

            ads.Activate();
            gateway.FinishBanner(true);

            Assert.That(ads.IsAvailable, Is.True);
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(gateway.InterstitialLoads, Is.EqualTo(1));
            Assert.That(gateway.RewardedLoads, Is.EqualTo(1));
            Assert.That(gateway.LoadedUnits, Is.EquivalentTo(new[] { "interstitial-unit", "rewarded-unit" }));
            Assert.That(gateway.Banners, Is.EqualTo(new[] { "banner-unit" }));
            Assert.That(Markers, Is.EqualTo(new[] { "MARKER CMPOSE_AD format=banner result=Shown" }));
        }

        [Test]
        public void Premium_skips_interstitials_grants_rewards_and_never_shows_a_banner()
        {
            premium.IsPremium = true;
            var off = Make();
            Assert.That(Run(off.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.Granted));
            var ads = Make();
            ads.Activate();
            ads.ShowBanner();

            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.Skipped));
            Assert.That(Run(ads.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.Granted));
            Assert.That(gateway.InterstitialLoads + gateway.RewardedLoads + gateway.InterstitialShows + gateway.RewardedShows, Is.Zero);
            Assert.That(gateway.Banners, Is.Empty);
        }

        [Test]
        public void Turning_premium_hides_the_banner_at_once_and_losing_it_brings_the_banner_back()
        {
            var ads = Make();
            ads.Activate();
            ads.ShowBanner();

            premium.IsPremium = true;
            premium.Raise();
            Assert.That(gateway.BannerHides, Is.EqualTo(1));

            premium.IsPremium = false;
            premium.Raise();
            Assert.That(gateway.Banners, Is.EqualTo(new[] { "banner-unit", "banner-unit" }));
        }

        [Test]
        public void A_shown_interstitial_is_loaded_again()
        {
            var ads = Make();
            ads.Activate();

            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.Shown));
            Assert.That(gateway.InterstitialShows, Is.EqualTo(1));
            Assert.That(gateway.InterstitialLoads, Is.EqualTo(2));
            Assert.That(Markers.Last(), Is.EqualTo("MARKER CMPOSE_AD format=interstitial result=Shown"));
        }

        [Test]
        public void Rewarded_reports_a_reward_a_dismissal_or_a_failure_and_loads_again()
        {
            var ads = Make();
            ads.Activate();

            Assert.That(Run(ads.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.Rewarded));
            gateway.RewardedReply = RewardResult.Dismissed;
            Assert.That(Run(ads.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.Dismissed));
            gateway.RewardedReply = RewardResult.Failed;
            Assert.That(Run(ads.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.Failed));
            Assert.That(gateway.RewardedShows, Is.EqualTo(3));
            Assert.That(gateway.RewardedLoads, Is.EqualTo(4));
        }

        [Test]
        public void A_second_request_while_an_ad_is_open_is_refused()
        {
            var ads = Make();
            ads.Activate();
            gateway.PendingInterstitial = new UniTaskCompletionSource<AdResult>();

            var first = ads.ShowInterstitialAsync(CancellationToken.None);
            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.NotReady));
            Assert.That(Run(ads.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.NotReady));
            gateway.PendingInterstitial.TrySetResult(AdResult.Shown);

            Assert.That(Run(first), Is.EqualTo(AdResult.Shown));
            Assert.That(gateway.InterstitialShows, Is.EqualTo(1));
            Assert.That(gateway.RewardedShows, Is.Zero);
        }

        [Test]
        public void A_failed_load_retries_three_times_with_growing_waits_then_waits_for_the_next_request()
        {
            gateway.InterstitialLoadSucceeds = false;
            var ads = Make();
            ads.Activate();

            Assert.That(gateway.InterstitialLoads, Is.EqualTo(4));
            Assert.That(waits, Is.EqualTo(new[] { 2, 4, 8 }));
            Assert.That(log.Lines, Has.Some.StartsWith("WARNING No interstitial ad after 4 tries"));

            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.NotReady));
            Assert.That(gateway.InterstitialLoads, Is.EqualTo(8));
            gateway.InterstitialLoadSucceeds = true;
            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.NotReady));
            Assert.That(gateway.InterstitialLoads, Is.EqualTo(9));
            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.Shown));
        }

        [Test]
        public void The_pacer_decides_which_interstitial_requests_show()
        {
            var ads = Make(every: 2);
            ads.Activate();

            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.Skipped));
            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.Shown));
            Assert.That(Run(ads.ShowInterstitialAsync(CancellationToken.None)), Is.EqualTo(AdResult.Skipped));
            Assert.That(gateway.InterstitialShows, Is.EqualTo(1));
        }

        [Test]
        public void Deactivation_hides_the_banner_and_refuses_ads()
        {
            var ads = Make();
            ads.Activate();
            ads.ShowBanner();
            var changes = 0;
            ads.Changed += () => changes++;

            ads.Deactivate();

            Assert.That(ads.IsAvailable, Is.False);
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(gateway.BannerHides, Is.EqualTo(1));
            Assert.That(Run(ads.ShowRewardedAsync(CancellationToken.None)), Is.EqualTo(RewardResult.NotReady));
        }

        [Test]
        public void Hiding_a_banner_that_is_not_shown_does_nothing()
        {
            var ads = Make();
            ads.Activate();

            ads.HideBanner();
            Assert.That(gateway.BannerHides, Is.Zero);
            ads.ShowBanner();
            ads.ShowBanner();
            ads.HideBanner();

            Assert.That(gateway.Banners.Count, Is.EqualTo(1));
            Assert.That(gateway.BannerHides, Is.EqualTo(1));
        }
    }
}
