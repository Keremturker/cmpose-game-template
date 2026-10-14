namespace Kturker.UnityBase.Core.Update
{
    // MinVersionUrl: AppConfig.minVersionUrl (blank: no forced update). BundleId: the App Store lookup's key.
    public sealed class UpdateConfig
    {
        public UpdateConfig(string minVersionUrl, string bundleId)
        {
            MinVersionUrl = (minVersionUrl ?? string.Empty).Trim();
            BundleId = (bundleId ?? string.Empty).Trim();
        }

        public string MinVersionUrl { get; }
        public string BundleId { get; }
    }
}
