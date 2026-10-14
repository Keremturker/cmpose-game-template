using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Update
{
    // What the update flow asks of the platform: Google Play In-App Updates on Android, the App Store on iOS
    public interface IPlatformUpdater
    {
        StorePlatform Platform { get; }

        AppVersion CurrentVersion { get; }

        // Android: starts Google Play's flexible update when Play offers one (true). Elsewhere: false.
        UniTask<bool> StartFlexibleUpdateAsync(CancellationToken cancellation);

        // Whether the Update button can do anything: iOS needs the App Store page from the lookup
        bool CanStartUpdate(string storeUrl);

        // The Update button: Android starts Google Play's immediate update (the Play Store page when Play offers
        // none); iOS opens storeUrl
        void StartUpdate(string storeUrl);
    }
}
