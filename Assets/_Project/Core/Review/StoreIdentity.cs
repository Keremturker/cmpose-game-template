using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Review
{
    // Where "Rate the app" goes: Android has a Play Store page for its bundle id; iOS needs AppConfig.appStoreId
    public sealed class StoreIdentity
    {
        public StoreIdentity(StorePlatform platform, string bundleId, string appStoreId)
        {
            Platform = platform;
            BundleId = (bundleId ?? string.Empty).Trim();
            AppStoreId = (appStoreId ?? string.Empty).Trim();
        }

        public StorePlatform Platform { get; }
        public string BundleId { get; }
        public string AppStoreId { get; }

        // Null when there is no page (iOS without an App Store ID, or no bundle id)
        public string PageUrl
        {
            get
            {
                if (Platform == StorePlatform.Ios) return StoreLinks.AppStoreWriteReview(AppStoreId);
                return BundleId.Length == 0 ? null : StoreLinks.PlayApp(BundleId);
            }
        }

        // Opened when no app can open PageUrl; Android only
        public string FallbackUrl => Platform == StorePlatform.Android && BundleId.Length > 0 ? StoreLinks.PlayWeb(BundleId) : null;

        public bool HasPage => PageUrl != null;
    }
}
