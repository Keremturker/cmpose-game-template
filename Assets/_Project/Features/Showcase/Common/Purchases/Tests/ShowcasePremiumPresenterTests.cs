using Kturker.UnityBase.Core.Purchases;
using Kturker.UnityBase.Core.Purchases.Tests;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Common.Purchases.Tests
{
    public sealed class ShowcasePremiumPresenterTests
    {
        private FakePremiumButtonView view;
        private FakePurchases purchases;
        private FakePaywall paywall;
        private FakeUiFeedback feedback;

        [SetUp]
        public void SetUp()
        {
            view = new FakePremiumButtonView();
            purchases = new FakePurchases();
            paywall = new FakePaywall();
            feedback = new FakeUiFeedback();
        }

        private ShowcasePremiumPresenter Create()
        {
            var presenter = new ShowcasePremiumPresenter(view, purchases, paywall, feedback);
            presenter.Start();
            return presenter;
        }

        [Test]
        public void Hidden_while_purchases_are_unavailable()
        {
            purchases.IsAvailable = false;
            Create();

            Assert.That(view.Visible, Is.False);
        }

        [Test]
        public void Shown_when_available_and_not_premium()
        {
            Create();

            Assert.That(view.Visible, Is.True);
        }

        [Test]
        public void Hidden_once_premium_arrives()
        {
            Create();

            purchases.IsPremium = true;
            purchases.RaiseChanged();

            Assert.That(view.Visible, Is.False);
        }

        [Test]
        public void A_tap_opens_the_paywall_from_the_menu()
        {
            Create();

            view.Click();

            Assert.That(paywall.Shown, Is.EqualTo(new[] { PaywallSource.Menu }));
            Assert.That(feedback.Clicks, Is.EqualTo(1));
        }

        [Test]
        public void After_dispose_nothing_reaches_the_view_or_the_paywall()
        {
            var presenter = Create();
            presenter.Dispose();

            purchases.IsPremium = true;
            purchases.RaiseChanged();
            view.Click();

            Assert.That(view.Visible, Is.True);
            Assert.That(paywall.Shown, Is.Empty);
        }
    }
}
