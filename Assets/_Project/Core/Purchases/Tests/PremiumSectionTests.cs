using NUnit.Framework;

namespace Kturker.UnityBase.Core.Purchases.Tests
{
    public sealed class PremiumSectionTests
    {
        [Test]
        public void The_row_shows_get_active_or_unavailable()
        {
            var purchases = new FakePurchases();
            var section = new PremiumSection(purchases, new FakePaywall());

            Assert.That(section.TitleKey, Is.EqualTo(PurchasesKeys.SettingsTitle));
            Assert.That(section.Options.Count, Is.EqualTo(1));
            Assert.That(section.Selected, Is.Zero);
            Assert.That(section.Options[0].LabelKey, Is.EqualTo(PurchasesKeys.StateGet));
            purchases.IsPremium = true;
            Assert.That(section.Options[0].LabelKey, Is.EqualTo(PurchasesKeys.StateActive));
            purchases.IsPremium = false;
            purchases.IsAvailable = false;
            Assert.That(section.Options[0].LabelKey, Is.EqualTo(PurchasesKeys.StateUnavailable));
        }

        [Test]
        public void A_press_opens_the_paywall_only_when_it_can_sell()
        {
            var purchases = new FakePurchases();
            var paywall = new FakePaywall();
            var section = new PremiumSection(purchases, paywall);

            section.Select(0);
            purchases.IsPremium = true;
            section.Select(0);
            purchases.IsPremium = false;
            purchases.IsAvailable = false;
            section.Select(0);
            purchases.IsAvailable = true;
            section.Select(1);

            Assert.That(paywall.Shown, Is.EqualTo(new[] { PaywallSource.Settings }));
        }

        [Test]
        public void Changed_follows_the_purchases()
        {
            var purchases = new FakePurchases();
            var section = new PremiumSection(purchases, new FakePaywall());
            var changes = 0;
            section.Changed += () => changes++;

            purchases.RaiseChanged();

            Assert.That(changes, Is.EqualTo(1));
        }
    }
}
