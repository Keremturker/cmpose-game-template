namespace Kturker.UnityBase.Core.Purchases
{
    // One plan as a paywall shows it; plain data, app code never sees a RevenueCat type
    public sealed class PaywallPackage
    {
        public PaywallPackage(string id, PackagePeriod period, string price, string pricePerMonth, int? savingsPercent, TrialPeriod? freeTrial)
        {
            Id = id;
            Period = period;
            Price = price;
            PricePerMonth = pricePerMonth;
            SavingsPercent = savingsPercent;
            FreeTrial = freeTrial;
        }

        // RevenueCat's package identifier ($rc_annual, $rc_monthly or your own): PurchaseAsync takes it
        public string Id { get; }
        public PackagePeriod Period { get; }
        // Formatted by the store ("$29.99", "₺349,99")
        public string Price { get; }
        // Annual and Weekly only: one month at this plan's rate, from the store; else null
        public string PricePerMonth { get; }
        // Annual only, when the offering also has Monthly in the same currency: what a year saves, in percent
        public int? SavingsPercent { get; }
        public TrialPeriod? FreeTrial { get; }
    }
}
