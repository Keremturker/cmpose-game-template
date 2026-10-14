using NUnit.Framework;

namespace Kturker.UnityBase.Core.Purchases.Tests
{
    public sealed class StoreErrorsTests
    {
        [Test]
        public void RevenueCat_error_codes_map_to_kinds_and_the_rest_is_other()
        {
            Assert.That(StoreErrors.Kind(1), Is.EqualTo(StoreErrorKind.Cancelled));
            Assert.That(StoreErrors.Kind(2), Is.EqualTo(StoreErrorKind.Store));
            Assert.That(StoreErrors.Kind(32), Is.EqualTo(StoreErrorKind.Store));
            Assert.That(StoreErrors.Kind(3), Is.EqualTo(StoreErrorKind.NotAllowed));
            Assert.That(StoreErrors.Kind(5), Is.EqualTo(StoreErrorKind.ProductUnavailable));
            Assert.That(StoreErrors.Kind(10), Is.EqualTo(StoreErrorKind.Network));
            Assert.That(StoreErrors.Kind(35), Is.EqualTo(StoreErrorKind.Network));
            Assert.That(StoreErrors.Kind(20), Is.EqualTo(StoreErrorKind.Pending));
            foreach (var code in new[] { 0, 4, 6, 11, 23, 99, -1 })
                Assert.That(StoreErrors.Kind(code), Is.EqualTo(StoreErrorKind.Other), code.ToString());
        }
    }
}
