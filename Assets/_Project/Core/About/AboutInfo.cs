using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.About
{
    // What the About screen shows; a blank legal URL hides its button
    public sealed class AboutInfo
    {
        public AboutInfo(string version, string privacyUrl, string termsUrl, string licenses)
        {
            Version = version ?? string.Empty;
            PrivacyUrl = (privacyUrl ?? string.Empty).Trim();
            TermsUrl = (termsUrl ?? string.Empty).Trim();
            Licenses = licenses ?? string.Empty;
        }

        public string Version { get; }
        public string PrivacyUrl { get; }
        public string TermsUrl { get; }
        public string Licenses { get; }
        public string VersionLabel => $"v{Version}";
        public bool HasPrivacy => PrivacyUrl.Length > 0;
        public bool HasTerms => TermsUrl.Length > 0;

        public static AboutInfo From(AppInfo app, AppConfig config) =>
            new AboutInfo(app.Version, config.PrivacyUrl, config.TermsUrl,
                config.ThirdPartyNotices != null ? config.ThirdPartyNotices.text : string.Empty);
    }
}
