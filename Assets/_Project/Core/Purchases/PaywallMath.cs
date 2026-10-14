using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kturker.UnityBase.Core.Purchases
{
    // Turns RevenueCat's packages into a PaywallOffer (cmpose's PackageMapping.kt, the same rules)
    public static class PaywallMath
    {
        private static readonly Regex Iso = new Regex("^P([0-9]+)([DWMY])$");

        public static PaywallOffer BuildOffer(IReadOnlyList<StorePackage> packages)
        {
            var annual = packages.FirstOrDefault(p => Period(p.PackageType) == PackagePeriod.Annual);
            var monthly = packages.FirstOrDefault(p => Period(p.PackageType) == PackagePeriod.Monthly);
            var savings = annual != null && monthly != null ? SavingsPercent(annual, monthly) : null;
            return new PaywallOffer(packages.Select(p =>
            {
                var period = Period(p.PackageType);
                var perMonth = (period == PackagePeriod.Annual || period == PackagePeriod.Weekly) && !string.IsNullOrEmpty(p.PricePerMonthString)
                    ? p.PricePerMonthString
                    : null;
                return new PaywallPackage(p.Id, period, p.PriceString, perMonth, ReferenceEquals(p, annual) ? savings : null,
                    Trial(p.TrialIsoPeriod));
            }).ToList());
        }

        public static PackagePeriod Period(string packageType)
        {
            switch (packageType)
            {
                case "WEEKLY": return PackagePeriod.Weekly;
                case "MONTHLY": return PackagePeriod.Monthly;
                case "ANNUAL": return PackagePeriod.Annual;
                case "LIFETIME": return PackagePeriod.Lifetime;
                default: return PackagePeriod.Other;
            }
        }

        // What Annual saves against twelve Monthly payments, rounded to a whole percent; null when the currencies
        // differ, a price is 0 or Annual saves nothing
        public static int? SavingsPercent(StorePackage annual, StorePackage monthly)
        {
            if (annual.CurrencyCode != monthly.CurrencyCode || annual.Price <= 0 || monthly.Price <= 0) return null;
            var yearOfMonthly = monthly.Price * 12;
            var percent = (int)Math.Round((yearOfMonthly - annual.Price) * 100 / yearOfMonthly, MidpointRounding.AwayFromZero);
            return percent > 0 ? percent : (int?)null;
        }

        // "P7D", "P1W", "P3M", "P1Y"; null for anything else (empty, zero, combined or time periods)
        public static TrialPeriod? Trial(string isoPeriod)
        {
            if (string.IsNullOrEmpty(isoPeriod)) return null;
            var match = Iso.Match(isoPeriod);
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out var value) || value <= 0) return null;
            switch (match.Groups[2].Value)
            {
                case "D": return new TrialPeriod(value, TrialUnit.Day);
                case "W": return new TrialPeriod(value, TrialUnit.Week);
                case "M": return new TrialPeriod(value, TrialUnit.Month);
                default: return new TrialPeriod(value, TrialUnit.Year);
            }
        }
    }
}
