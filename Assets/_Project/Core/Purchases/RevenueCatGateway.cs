using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Sdk = global::Purchases;

namespace Kturker.UnityBase.Core.Purchases
{
    // The only class that sees RevenueCat's Unity SDK. Purchases makes its native wrapper in Start, so Configure
    // waits one frame after adding it; every call waits for Configure. The SDK keeps one callback per API, so
    // overlapping offering and customer-info calls share one SDK call. Sdk is RevenueCat's global Purchases class,
    // whose name this namespace hides.
    public sealed class RevenueCatGateway : IRevenueCatGateway
    {
        public const string ObjectName = "CmposePurchases";

        private static readonly IReadOnlyCollection<string> None = new string[0];

        private readonly UniTaskCompletionSource ready = new UniTaskCompletionSource();
        private readonly Dictionary<string, Sdk.Package> packages = new Dictionary<string, Sdk.Package>();
        private Sdk purchases;
        private UniTask<StoreReply<IReadOnlyList<StorePackage>>> offerings;
        private bool offeringsRunning;
        private UniTask<StoreReply<IReadOnlyCollection<string>>> customer;
        private bool customerRunning;

        public void Configure(string apiKey, Action<IReadOnlyCollection<string>> customerInfoUpdated)
        {
            if (purchases != null) return;
            var go = new GameObject(ObjectName);
            go.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(go);
            purchases = go.AddComponent<Sdk>();
            purchases.useRuntimeSetup = true;
            var listener = go.AddComponent<CustomerInfoListener>();
            listener.Received = info => customerInfoUpdated(Active(info));
            purchases.listener = listener;
            go.SetActive(true);
            ConfigureNextFrameAsync(apiKey).Forget();
        }

        public UniTask<StoreReply<IReadOnlyList<StorePackage>>> OfferingsAsync()
        {
            if (!offeringsRunning)
            {
                offeringsRunning = true;
                offerings = FetchOfferingsAsync().Preserve();
            }
            return offerings;
        }

        public async UniTask<StoreReply<IReadOnlyCollection<string>>> PurchaseAsync(string packageId)
        {
            await ready.Task;
            if (!packages.TryGetValue(packageId, out var package))
                return StoreReply<IReadOnlyCollection<string>>.Failed(StoreErrorKind.ProductUnavailable);
            var source = new UniTaskCompletionSource<StoreReply<IReadOnlyCollection<string>>>();
            purchases.PurchasePackage(package, result =>
            {
                if (result.UserCancelled) source.TrySetResult(StoreReply<IReadOnlyCollection<string>>.Failed(StoreErrorKind.Cancelled));
                else if (result.Error != null) source.TrySetResult(StoreReply<IReadOnlyCollection<string>>.Failed(StoreErrors.Kind(result.Error.Code)));
                else source.TrySetResult(StoreReply<IReadOnlyCollection<string>>.Ok(Active(result.CustomerInfo)));
            });
            return await source.Task;
        }

        public async UniTask<StoreReply<IReadOnlyCollection<string>>> RestoreAsync()
        {
            await ready.Task;
            var source = new UniTaskCompletionSource<StoreReply<IReadOnlyCollection<string>>>();
            purchases.RestorePurchases((info, error) => source.TrySetResult(CustomerReply(info, error)));
            return await source.Task;
        }

        public UniTask<StoreReply<IReadOnlyCollection<string>>> CustomerInfoAsync()
        {
            if (!customerRunning)
            {
                customerRunning = true;
                customer = FetchCustomerAsync().Preserve();
            }
            return customer;
        }

        private async UniTaskVoid ConfigureNextFrameAsync(string apiKey)
        {
            await UniTask.NextFrame();
            purchases.Configure(Sdk.PurchasesConfiguration.Builder.Init(apiKey).Build());
            ready.TrySetResult();
        }

        private async UniTask<StoreReply<IReadOnlyList<StorePackage>>> FetchOfferingsAsync()
        {
            try
            {
                await ready.Task;
                var source = new UniTaskCompletionSource<StoreReply<IReadOnlyList<StorePackage>>>();
                purchases.GetOfferings((all, error) =>
                {
                    if (error != null)
                    {
                        source.TrySetResult(StoreReply<IReadOnlyList<StorePackage>>.Failed(StoreErrors.Kind(error.Code)));
                        return;
                    }
                    packages.Clear();
                    var current = all?.Current;
                    if (current == null)
                    {
                        source.TrySetResult(StoreReply<IReadOnlyList<StorePackage>>.Ok(null));
                        return;
                    }
                    foreach (var package in current.AvailablePackages) packages[package.Identifier] = package;
                    source.TrySetResult(StoreReply<IReadOnlyList<StorePackage>>.Ok(current.AvailablePackages.Select(ToStorePackage).ToList()));
                });
                return await source.Task;
            }
            finally
            {
                offeringsRunning = false;
            }
        }

        private async UniTask<StoreReply<IReadOnlyCollection<string>>> FetchCustomerAsync()
        {
            try
            {
                await ready.Task;
                var source = new UniTaskCompletionSource<StoreReply<IReadOnlyCollection<string>>>();
                purchases.GetCustomerInfo((info, error) => source.TrySetResult(CustomerReply(info, error)));
                return await source.Task;
            }
            finally
            {
                customerRunning = false;
            }
        }

        private static StoreReply<IReadOnlyCollection<string>> CustomerReply(Sdk.CustomerInfo info, Sdk.Error error) =>
            error != null
                ? StoreReply<IReadOnlyCollection<string>>.Failed(StoreErrors.Kind(error.Code))
                : StoreReply<IReadOnlyCollection<string>>.Ok(Active(info));

        // RevenueCat's model classes are plain C# objects, so ?. is safe here
        private static IReadOnlyCollection<string> Active(Sdk.CustomerInfo info) =>
            info?.Entitlements?.Active?.Keys.ToList() ?? None;

        // A free introductory price is a free trial (iOS introductory offer, Android free phase)
        private static StorePackage ToStorePackage(Sdk.Package package)
        {
            var product = package.StoreProduct;
            var intro = product.IntroductoryPrice;
            var trial = intro != null && intro.Price <= 0f ? intro.Period : null;
            return new StorePackage(package.Identifier, package.PackageType, product.PriceString, product.Price,
                product.CurrencyCode, product.PricePerMonthString, trial);
        }
    }
}
