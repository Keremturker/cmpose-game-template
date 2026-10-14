namespace Kturker.UnityBase.Features.Shell.Purchases
{
    // One package row: period name (key), store price, an optional detail line (trial or monthly price, key + {0})
    // and an optional savings badge
    public readonly struct PaywallRow
    {
        public PaywallRow(string periodKey, string price, string detailKey, string detailArg, int? savingsPercent)
        {
            PeriodKey = periodKey;
            Price = price;
            DetailKey = detailKey;
            DetailArg = detailArg;
            SavingsPercent = savingsPercent;
        }

        public string PeriodKey { get; }
        public string Price { get; }
        public string DetailKey { get; }
        public string DetailArg { get; }
        public int? SavingsPercent { get; }
    }
}
