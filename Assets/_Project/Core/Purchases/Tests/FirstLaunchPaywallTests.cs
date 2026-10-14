using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Purchases.Tests
{
    public sealed class FirstLaunchPaywallTests
    {
        private static FirstLaunchPaywall Make(out FakeSceneLoader loader, out FakePurchases purchases, out FakePaywall paywall,
            out FakePreferences preferences)
        {
            loader = new FakeSceneLoader();
            purchases = new FakePurchases();
            paywall = new FakePaywall();
            preferences = new FakePreferences();
            var first = new FirstLaunchPaywall(loader, purchases, paywall, preferences);
            first.Start();
            return first;
        }

        [Test]
        public void The_first_menu_shows_the_paywall_once_and_remembers_it()
        {
            Make(out var loader, out _, out var paywall, out var preferences);

            loader.LoadAsync(SceneId.Menu);
            loader.LoadAsync(SceneId.Game);
            loader.LoadAsync(SceneId.Menu);

            Assert.That(paywall.Shown, Is.EqualTo(new[] { PaywallSource.First }));
            Assert.That(preferences.Values[FirstLaunchPaywall.ShownKey], Is.EqualTo(true));
            Assert.That(preferences.Saves, Is.EqualTo(1));
        }

        [Test]
        public void Another_scene_first_waits_for_the_menu()
        {
            Make(out var loader, out _, out var paywall, out _);

            loader.LoadAsync(SceneId.Game);
            Assert.That(paywall.Shown, Is.Empty);
            loader.LoadAsync(SceneId.Menu);
            Assert.That(paywall.Shown, Is.EqualTo(new[] { PaywallSource.First }));
        }

        [Test]
        public void Never_again_once_shown_in_an_earlier_run()
        {
            var loader = new FakeSceneLoader();
            var paywall = new FakePaywall();
            var preferences = new FakePreferences();
            preferences.SetBool(FirstLaunchPaywall.ShownKey, true);
            new FirstLaunchPaywall(loader, new FakePurchases(), paywall, preferences).Start();

            loader.LoadAsync(SceneId.Menu);

            Assert.That(paywall.Shown, Is.Empty);
        }

        [Test]
        public void Nothing_when_unavailable_or_premium_and_nothing_after_dispose()
        {
            Make(out var loader, out var purchases, out var paywall, out var preferences);
            purchases.IsAvailable = false;
            loader.LoadAsync(SceneId.Menu);

            var premium = Make(out var premiumLoader, out var premiumPurchases, out var premiumPaywall, out _);
            premiumPurchases.IsPremium = true;
            premiumLoader.LoadAsync(SceneId.Menu);

            var disposed = Make(out var disposedLoader, out _, out var disposedPaywall, out _);
            disposed.Dispose();
            disposedLoader.LoadAsync(SceneId.Menu);

            Assert.That(paywall.Shown, Is.Empty);
            Assert.That(premiumPaywall.Shown, Is.Empty);
            Assert.That(disposedPaywall.Shown, Is.Empty);
            Assert.That(preferences.Values.ContainsKey(FirstLaunchPaywall.ShownKey), Is.False);
            premium.Dispose();
        }
    }
}
