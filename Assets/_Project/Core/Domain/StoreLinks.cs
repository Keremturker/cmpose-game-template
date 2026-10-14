namespace Kturker.UnityBase.Core.Domain
{
    // Store page URLs. Android: the Play Store app, then the web page. iOS: the App Store's write-a-review page, which
    // needs the numeric App Store ID (null while it is blank).
    public static class StoreLinks
    {
        public static string PlayApp(string bundleId) => $"market://details?id={bundleId}";

        public static string PlayWeb(string bundleId) => $"https://play.google.com/store/apps/details?id={bundleId}";

        public static string AppStoreWriteReview(string appStoreId)
        {
            var id = (appStoreId ?? string.Empty).Trim();
            return id.Length == 0 ? null : $"itms-apps://apps.apple.com/app/id{id}?action=write-review";
        }
    }
}
