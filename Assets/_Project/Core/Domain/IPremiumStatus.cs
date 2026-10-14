using System;

namespace Kturker.UnityBase.Core.Domain
{
    // Whether the player has premium (no ads). A flag that sells premium registers one (purchases: RevenueCatPurchases);
    // without such a flag nothing is registered and nobody is premium.
    public interface IPremiumStatus
    {
        bool IsPremium { get; }

        // After IsPremium changes
        event Action Changed;
    }
}
