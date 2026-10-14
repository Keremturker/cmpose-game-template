using System.Linq;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Purchases.Tests
{
    public sealed class PaywallMathTests
    {
        private static StorePackage Package(string id, string type, double price = 1.0, string currency = "USD",
            string perMonth = null, string trial = null) =>
            new StorePackage(id, type, $"price-{id}", price, currency, perMonth, trial);

        [Test]
        public void Keeps_the_offering_order_and_the_store_prices()
        {
            var offer = PaywallMath.BuildOffer(new[] { Package("m", "MONTHLY"), Package("a", "ANNUAL") });

            Assert.That(offer.Packages.Select(p => p.Id), Is.EqualTo(new[] { "m", "a" }));
            Assert.That(offer.Packages.Select(p => p.Price), Is.EqualTo(new[] { "price-m", "price-a" }));
        }

        [Test]
        public void Annual_saves_against_twelve_monthly_payments()
        {
            var offer = PaywallMath.BuildOffer(new[] { Package("m", "MONTHLY", 9.99), Package("a", "ANNUAL", 59.99) });

            // 12 x 9.99 = 119.88; (119.88 - 59.99) / 119.88 = 49.96 % -> 50
            Assert.That(offer.Packages.Single(p => p.Id == "a").SavingsPercent, Is.EqualTo(50));
            Assert.That(offer.Packages.Single(p => p.Id == "m").SavingsPercent, Is.Null);
        }

        [Test]
        public void Savings_round_to_the_nearest_percent_and_float_prices_do_not_change_them()
        {
            var monthly = Package("m", "MONTHLY", 10.0);
            // (120 - 100) / 120 = 16.67 % -> 17
            Assert.That(PaywallMath.SavingsPercent(Package("a", "ANNUAL", 100.0), monthly), Is.EqualTo(17));
            // (120 - 119.5) / 120 = 0.42 % -> 0 -> no badge
            Assert.That(PaywallMath.SavingsPercent(Package("a", "ANNUAL", 119.5), monthly), Is.Null);
            // The SDK hands prices over as float: 19.99f is 19.9899997711… and still saves 44 % against 2.99
            Assert.That(PaywallMath.SavingsPercent(Package("a", "ANNUAL", 19.99f), Package("m", "MONTHLY", 2.99f)), Is.EqualTo(44));
        }

        [Test]
        public void No_savings_when_annual_costs_as_much_or_more()
        {
            var monthly = Package("m", "MONTHLY", 1.0);

            Assert.That(PaywallMath.SavingsPercent(Package("a", "ANNUAL", 12.0), monthly), Is.Null);
            Assert.That(PaywallMath.SavingsPercent(Package("a", "ANNUAL", 13.0), monthly), Is.Null);
        }

        [Test]
        public void No_savings_without_a_monthly_package()
        {
            var offer = PaywallMath.BuildOffer(new[] { Package("w", "WEEKLY"), Package("a", "ANNUAL") });

            Assert.That(offer.Packages.Single(p => p.Id == "a").SavingsPercent, Is.Null);
        }

        [Test]
        public void No_savings_across_currencies_or_with_a_free_price()
        {
            var annual = Package("a", "ANNUAL", 50.0);

            Assert.That(PaywallMath.SavingsPercent(annual, Package("m", "MONTHLY", 9.99, currency: "EUR")), Is.Null);
            Assert.That(PaywallMath.SavingsPercent(annual, Package("m", "MONTHLY", 0.0)), Is.Null);
            Assert.That(PaywallMath.SavingsPercent(Package("a", "ANNUAL", 0.0), Package("m", "MONTHLY", 9.99)), Is.Null);
        }

        [Test]
        public void Price_per_month_only_for_annual_and_weekly()
        {
            var offer = PaywallMath.BuildOffer(new[]
            {
                Package("w", "WEEKLY", perMonth: "w/mo"), Package("m", "MONTHLY", perMonth: "m/mo"),
                Package("a", "ANNUAL", perMonth: "a/mo"), Package("l", "LIFETIME", perMonth: "l/mo"),
                Package("e", "ANNUAL", perMonth: "")
            });

            Assert.That(offer.Packages.Select(p => p.PricePerMonth), Is.EqualTo(new[] { "w/mo", null, "a/mo", null, null }));
        }

        [Test]
        public void Lifetime_has_no_savings_or_monthly_price_and_other_types_are_other()
        {
            var lifetime = PaywallMath.BuildOffer(new[] { Package("l", "LIFETIME") }).Packages.Single();
            Assert.That(lifetime.Period, Is.EqualTo(PackagePeriod.Lifetime));
            Assert.That(lifetime.SavingsPercent, Is.Null);
            Assert.That(lifetime.PricePerMonth, Is.Null);

            foreach (var type in new[] { "UNKNOWN", "CUSTOM", "SIX_MONTH", "THREE_MONTH", "TWO_MONTH", "", null, "weekly" })
                Assert.That(PaywallMath.Period(type), Is.EqualTo(PackagePeriod.Other), type ?? "null");
            Assert.That(PaywallMath.Period("WEEKLY"), Is.EqualTo(PackagePeriod.Weekly));
            Assert.That(PaywallMath.Period("MONTHLY"), Is.EqualTo(PackagePeriod.Monthly));
            Assert.That(PaywallMath.Period("ANNUAL"), Is.EqualTo(PackagePeriod.Annual));
        }

        [Test]
        public void Trial_periods_from_iso_8601()
        {
            Assert.That(PaywallMath.Trial("P7D"), Is.EqualTo(new TrialPeriod(7, TrialUnit.Day)));
            Assert.That(PaywallMath.Trial("P1W"), Is.EqualTo(new TrialPeriod(1, TrialUnit.Week)));
            Assert.That(PaywallMath.Trial("P3M"), Is.EqualTo(new TrialPeriod(3, TrialUnit.Month)));
            Assert.That(PaywallMath.Trial("P1Y"), Is.EqualTo(new TrialPeriod(1, TrialUnit.Year)));
            foreach (var bad in new[] { null, "", "P0D", "P1Y2M", "7D", "P1H", "PT5M", "P-1D", "garbage" })
                Assert.That(PaywallMath.Trial(bad), Is.Null, bad ?? "null");
        }

        [Test]
        public void The_offer_carries_the_free_trial()
        {
            var offer = PaywallMath.BuildOffer(new[] { Package("a", "ANNUAL", trial: "P1W"), Package("m", "MONTHLY") });

            Assert.That(offer.Packages[0].FreeTrial, Is.EqualTo(new TrialPeriod(1, TrialUnit.Week)));
            Assert.That(offer.Packages[1].FreeTrial, Is.Null);
        }
    }
}
