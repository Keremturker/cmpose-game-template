using System;
using System.Text.RegularExpressions;
using Kturker.UnityBase.Editor.Build;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class PodfilePatchTests
    {
        // The shape EDM4U writes (git Specs source, no post_install)
        private const string Podfile = "source 'https://github.com/CocoaPods/Specs'\n" +
            "platform :ios, '15.0'\n\n" +
            "target 'UnityFramework' do\n" +
            "  pod 'PurchasesHybridCommon', '19.5.0'\n" +
            "end\n" +
            "target 'Unity-iPhone' do\n" +
            "end\n" +
            "use_frameworks! :linkage => :static\n";

        private static int Count(string text, string part) => Regex.Matches(text, Regex.Escape(part)).Count;

        [Test]
        public void Only_the_cdn_is_a_source_and_every_pod_target_moves_to_ios_15()
        {
            var patched = PodfilePatch.Apply(Podfile);

            Assert.That(patched, Does.StartWith(PodfilePatch.Cdn + "\n"));
            Assert.That(Count(patched, "source '"), Is.EqualTo(1));
            Assert.That(patched, Does.Not.Contain("github.com/CocoaPods/Specs"));
            Assert.That(Count(patched, "post_install do |installer|"), Is.EqualTo(1));
            Assert.That(patched, Does.Contain("config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '15.0'"));
            Assert.That(patched, Does.Contain("pod 'PurchasesHybridCommon', '19.5.0'"));
        }

        [Test]
        public void Google_mobile_ads_is_linked_after_the_swift_pods_built_here()
        {
            var patched = PodfilePatch.Apply(Podfile);

            Assert.That(patched, Does.Contain("installer.aggregate_targets.each do |aggregate|"));
            Assert.That(patched, Does.Contain("fixed = text.gsub(/^(OTHER_LDFLAGS = .*?) -framework \"GoogleMobileAds\"(.*)$/) " +
                "{ \"#{$1}#{$2} -framework \\\"GoogleMobileAds\\\"\" }"));
            Assert.That(patched.IndexOf("aggregate_targets", StringComparison.Ordinal),
                Is.GreaterThan(patched.IndexOf("IPHONEOS_DEPLOYMENT_TARGET", StringComparison.Ordinal)));
            Assert.That(patched.TrimEnd('\n'), Does.EndWith("end"));
        }

        [Test]
        public void Applying_twice_changes_nothing()
        {
            var once = PodfilePatch.Apply(Podfile);

            Assert.That(PodfilePatch.Apply(once), Is.EqualTo(once));
        }

        [Test]
        public void A_post_install_hook_of_someone_else_fails_loudly()
        {
            var foreign = Podfile + "post_install do |installer|\n  puts 'hi'\nend\n";

            var error = Assert.Throws<InvalidOperationException>(() => PodfilePatch.Apply(foreign));
            Assert.That(error.Message, Does.Contain("post_install"));
        }
    }
}
