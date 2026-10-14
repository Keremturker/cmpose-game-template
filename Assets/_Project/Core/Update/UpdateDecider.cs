using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Update
{
    // Below the minimum: Required. A newer version on the App Store: Available (iOS; on Android Google Play offers its
    // own flexible update instead). Anything unknown (no file, no lookup answer, no version code): None.
    public static class UpdateDecider
    {
        public static UpdateDecision Decide(AppVersion current, MinVersions minVersions, StoreRelease storeRelease, StorePlatform platform)
        {
            var storeUrl = storeRelease != null ? storeRelease.StoreUrl : null;
            var belowMinimum = platform == StorePlatform.Android
                ? IsBelowAndroidMinimum(current, minVersions)
                : IsBelowIosMinimum(current, minVersions);
            if (belowMinimum) return UpdateDecision.Required(storeUrl);
            if (storeRelease != null && VersionName.Compare(storeRelease.Version, current.Name) > 0) return UpdateDecision.Available(storeUrl);
            return UpdateDecision.None;
        }

        private static bool IsBelowAndroidMinimum(AppVersion current, MinVersions minVersions) =>
            current.Code.HasValue && minVersions != null && minVersions.Android.HasValue && current.Code.Value < minVersions.Android.Value;

        private static bool IsBelowIosMinimum(AppVersion current, MinVersions minVersions) =>
            current.Name.Trim().Length > 0 && minVersions != null && !string.IsNullOrWhiteSpace(minVersions.Ios)
            && VersionName.Compare(current.Name, minVersions.Ios) < 0;
    }
}
