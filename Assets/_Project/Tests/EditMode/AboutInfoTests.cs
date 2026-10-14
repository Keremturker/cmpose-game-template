using System.IO;
using Kturker.UnityBase.Core.About;
using Kturker.UnityBase.Core.Domain;
using NUnit.Framework;
using UnityEditor;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class AboutInfoTests
    {
        [Test]
        public void Blank_legal_urls_hide_their_buttons()
        {
            var blank = new AboutInfo("1.0.0", "  ", null, "");
            Assert.That(blank.HasPrivacy, Is.False);
            Assert.That(blank.HasTerms, Is.False);

            var set = new AboutInfo("1.0.0", " https://example.com/privacy ", "https://example.com/terms", "");
            Assert.That(set.HasPrivacy, Is.True);
            Assert.That(set.HasTerms, Is.True);
            Assert.That(set.PrivacyUrl, Is.EqualTo("https://example.com/privacy"));
        }

        [Test]
        public void The_template_config_gives_the_version_label_licences_and_no_legal_links()
        {
            var config = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/_Project/Core/About/AppConfig.asset");
            Assert.That(config, Is.Not.Null);

            var info = AboutInfo.From(new AppInfo("UnityBase", "1.0.0", "Android"), config);

            Assert.That(info.VersionLabel, Is.EqualTo("v1.0.0"));
            Assert.That(info.Licenses, Does.Contain("VContainer").And.Contain("UniTask"));
            Assert.That(info.HasPrivacy, Is.False);
            Assert.That(info.HasTerms, Is.False);
        }

        // The generator edits these two lines, so they must exist in the asset
        [Test]
        public void The_template_config_leaves_the_store_settings_blank()
        {
            const string path = "Assets/_Project/Core/About/AppConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<AppConfig>(path);

            Assert.That(config.MinVersionUrl, Is.Empty);
            Assert.That(config.AppStoreId, Is.Empty);
            var text = File.ReadAllText(path);
            Assert.That(text, Does.Match(@"(?m)^  minVersionUrl: ?$"));
            Assert.That(text, Does.Match(@"(?m)^  appStoreId: ?$"));
        }
    }
}
