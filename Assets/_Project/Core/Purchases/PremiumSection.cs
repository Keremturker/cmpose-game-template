using System;
using System.Collections.Generic;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Core.Purchases
{
    // The Premium row in Settings: one option that shows the state (Get, Active, Unavailable); a press opens the
    // paywall when there is something to sell
    public sealed class PremiumSection : ISettingsSection
    {
        private static readonly SettingsOption[] Get = { SettingsOption.Key(PurchasesKeys.StateGet) };
        private static readonly SettingsOption[] Active = { SettingsOption.Key(PurchasesKeys.StateActive) };
        private static readonly SettingsOption[] Unavailable = { SettingsOption.Key(PurchasesKeys.StateUnavailable) };

        private readonly IPurchases purchases;
        private readonly IPaywall paywall;

        public PremiumSection(IPurchases purchases, IPaywall paywall)
        {
            this.purchases = purchases;
            this.paywall = paywall;
            purchases.Changed += OnChanged;
        }

        public string TitleKey => PurchasesKeys.SettingsTitle;
        public IReadOnlyList<SettingsOption> Options => !purchases.IsAvailable ? Unavailable : purchases.IsPremium ? Active : Get;
        public int Selected => 0;

        public event Action Changed;

        public void Select(int index)
        {
            if (index != 0 || !purchases.IsAvailable || purchases.IsPremium) return;
            paywall.Show(PaywallSource.Settings);
        }

        private void OnChanged() => Changed?.Invoke();
    }
}
