namespace Kturker.UnityBase.Core.Purchases
{
    // One RevenueCat package as the gateway reads it, so PaywallMath's rules are tested without the SDK
    public sealed class StorePackage
    {
        public StorePackage(string id, string packageType, string priceString, double price, string currencyCode,
            string pricePerMonthString, string trialIsoPeriod)
        {
            Id = id;
            PackageType = packageType;
            PriceString = priceString;
            Price = price;
            CurrencyCode = currencyCode;
            PricePerMonthString = pricePerMonthString;
            TrialIsoPeriod = trialIsoPeriod;
        }

        public string Id { get; }
        // RevenueCat's type name: WEEKLY, MONTHLY, ANNUAL, LIFETIME, SIX_MONTH, CUSTOM, UNKNOWN …
        public string PackageType { get; }
        public string PriceString { get; }
        public double Price { get; }
        public string CurrencyCode { get; }
        public string PricePerMonthString { get; }
        // ISO 8601 length of a free trial ("P1W"), null without one
        public string TrialIsoPeriod { get; }
    }
}
