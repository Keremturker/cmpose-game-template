using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Core.Purchases;

namespace Kturker.UnityBase.Features.Showcase.Common.Purchases
{
    // The purchases flag in the showcase: a Premium row in the menu opens the paywall. It hides while purchases are
    // unavailable (the editor, a Test Store key in release) and once the player has premium.
    public sealed class ShowcasePremiumPresenter : Presenter<IPremiumButtonView>
    {
        private readonly IPurchases purchases;
        private readonly IPaywall paywall;
        private readonly IUiFeedback feedback;

        public ShowcasePremiumPresenter(IPremiumButtonView view, IPurchases purchases, IPaywall paywall, IUiFeedback feedback)
            : base(view)
        {
            this.purchases = purchases;
            this.paywall = paywall;
            this.feedback = feedback;
        }

        public override void Start()
        {
            purchases.Changed += Refresh;
            View.Clicked += OnClicked;
            Refresh();
        }

        public override void Dispose()
        {
            purchases.Changed -= Refresh;
            View.Clicked -= OnClicked;
        }

        private void Refresh() => View.SetVisible(purchases.IsAvailable && !purchases.IsPremium);

        private void OnClicked()
        {
            feedback.Click();
            paywall.Show(PaywallSource.Menu);
        }
    }
}
