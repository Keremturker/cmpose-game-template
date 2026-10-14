using System.Collections.Generic;

namespace Kturker.UnityBase.Core.Purchases
{
    // The current offering's packages, in the order of the RevenueCat dashboard
    public sealed class PaywallOffer
    {
        public PaywallOffer(IReadOnlyList<PaywallPackage> packages) => Packages = packages;

        public IReadOnlyList<PaywallPackage> Packages { get; }
    }
}
