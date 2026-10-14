using UnityEngine;

namespace Kturker.UnityBase.Core.About
{
    // Per-game settings the generator edits: the legal URLs on the About screen (blank hides the button), the
    // minimum-version file (blank: no forced update) and the numeric App Store ID (blank hides "Rate the app" on iOS).
    // The asset is Assets/_Project/Core/About/AppConfig.asset, set on the root scope prefab.
    public sealed class AppConfig : ScriptableObject
    {
        [SerializeField] private string privacyUrl = "";
        [SerializeField] private string termsUrl = "";
        [SerializeField] private string minVersionUrl = "";
        [SerializeField] private string appStoreId = "";
        [SerializeField] private TextAsset thirdPartyNotices;

        public string PrivacyUrl => privacyUrl;
        public string TermsUrl => termsUrl;
        public string MinVersionUrl => minVersionUrl;
        public string AppStoreId => appStoreId;
        public TextAsset ThirdPartyNotices => thirdPartyNotices;
    }
}
