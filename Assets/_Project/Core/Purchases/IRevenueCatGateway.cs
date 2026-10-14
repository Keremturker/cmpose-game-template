using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Purchases
{
    // RevenueCat's SDK behind plain types (RevenueCatGateway is the only class that sees the SDK). Customer info is the
    // set of active entitlement ids. Calls made before Configure has finished wait for it.
    public interface IRevenueCatGateway
    {
        void Configure(string apiKey, Action<IReadOnlyCollection<string>> customerInfoUpdated);

        // Ok(null): RevenueCat has no current offering
        UniTask<StoreReply<IReadOnlyList<StorePackage>>> OfferingsAsync();

        // packageId must come from the last OfferingsAsync
        UniTask<StoreReply<IReadOnlyCollection<string>>> PurchaseAsync(string packageId);

        UniTask<StoreReply<IReadOnlyCollection<string>>> RestoreAsync();

        UniTask<StoreReply<IReadOnlyCollection<string>>> CustomerInfoAsync();
    }
}
