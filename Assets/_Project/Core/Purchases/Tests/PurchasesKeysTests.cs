using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Purchases.Tests
{
    public sealed class PurchasesKeysTests
    {
        [Test]
        public void Every_key_has_an_english_text()
        {
            var english = new TableLocalizer(new ResourcesStringSource(), new FakeLog(), false);
            var keys = new[]
            {
                PurchasesKeys.SettingsTitle, PurchasesKeys.StateGet, PurchasesKeys.StateActive, PurchasesKeys.StateUnavailable,
                PurchasesKeys.Title, PurchasesKeys.Feature1, PurchasesKeys.Feature2, PurchasesKeys.PerMonth, PurchasesKeys.Savings,
                PurchasesKeys.Buy, PurchasesKeys.Restore, PurchasesKeys.Unavailable, PurchasesKeys.ErrorUnavailable,
                PurchasesKeys.ErrorPackageUnavailable, PurchasesKeys.ErrorNothingToRestore
            };
            foreach (var key in keys) Assert.That(english.Get(key), Is.Not.EqualTo(key), key);
            foreach (PackagePeriod period in Enum.GetValues(typeof(PackagePeriod)))
                Assert.That(english.Get(PurchasesKeys.Period(period)), Is.Not.EqualTo(PurchasesKeys.Period(period)), period.ToString());
            foreach (StoreErrorKind kind in Enum.GetValues(typeof(StoreErrorKind)))
                Assert.That(english.Get(PurchasesKeys.Error(kind)), Is.Not.EqualTo(PurchasesKeys.Error(kind)), kind.ToString());
            foreach (TrialUnit unit in Enum.GetValues(typeof(TrialUnit)))
            {
                foreach (var count in new[] { 1, 3 })
                {
                    var key = PurchasesKeys.Trial(new TrialPeriod(count, unit));
                    Assert.That(english.Get(key, count), Does.Contain(count.ToString()), key);
                }
            }
            Assert.That(english.Get(PurchasesKeys.Savings, 44), Does.Contain("44"));
            Assert.That(english.Get(PurchasesKeys.PerMonth, "$4.99"), Does.Contain("$4.99"));
        }
    }
}
