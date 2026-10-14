namespace Kturker.UnityBase.Core.Purchases
{
    // Keys of Resources/Strings/<language>/purchases.json
    public static class PurchasesKeys
    {
        public const string SettingsTitle = "settings.premium";
        public const string StateGet = "purchases.state.get";
        public const string StateActive = "purchases.state.active";
        public const string StateUnavailable = "purchases.state.unavailable";
        public const string Title = "purchases.title";
        public const string Feature1 = "purchases.feature1";
        public const string Feature2 = "purchases.feature2";
        public const string PerMonth = "purchases.perMonth";
        public const string Savings = "purchases.savings";
        public const string Buy = "purchases.buy";
        public const string Restore = "purchases.restore";
        public const string Unavailable = "purchases.unavailable";
        public const string ErrorUnavailable = "purchases.error.unavailable";
        public const string ErrorPackageUnavailable = "purchases.error.packageUnavailable";
        public const string ErrorNothingToRestore = "purchases.error.nothingToRestore";

        public static string Period(PackagePeriod period)
        {
            switch (period)
            {
                case PackagePeriod.Weekly: return "purchases.period.weekly";
                case PackagePeriod.Monthly: return "purchases.period.monthly";
                case PackagePeriod.Annual: return "purchases.period.annual";
                case PackagePeriod.Lifetime: return "purchases.period.lifetime";
                default: return "purchases.period.other";
            }
        }

        // purchases.trial.<day|week|month|year>.<one|other>, with the number as {0}
        public static string Trial(TrialPeriod trial) =>
            $"purchases.trial.{trial.Unit.ToString().ToLowerInvariant()}.{(trial.Value == 1 ? "one" : "other")}";

        public static string Error(StoreErrorKind kind)
        {
            switch (kind)
            {
                case StoreErrorKind.Network: return "purchases.error.network";
                case StoreErrorKind.Pending: return "purchases.error.pending";
                case StoreErrorKind.Store: return "purchases.error.store";
                case StoreErrorKind.NotAllowed: return "purchases.error.notAllowed";
                case StoreErrorKind.ProductUnavailable: return ErrorPackageUnavailable;
                default: return "purchases.error.failed";
            }
        }
    }
}
