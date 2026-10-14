using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kturker.UnityBase.Editor.Build
{
    // Three fixes to the Podfile EDM4U writes: the CocoaPods CDN as the only source (the git Specs repo makes the first
    // pod install take about ten minutes), every Pod target on iOS 15.0 (Xcode 27 refuses older targets) and Google
    // Mobile Ads linked last. Its prebuilt binary carries an older Swift compiler's copy of a runtime helper that every
    // Swift object defines; the static linker keeps the first copy it meets, and RevenueCat (built here by Xcode 27)
    // crashes reading type metadata through the old one. Linking it after the Swift pods built here keeps the newer copy.
    // Applying it twice changes nothing; a post_install hook from someone else is an error, Ruby allows only one.
    public static class PodfilePatch
    {
        public const string MinimumIos = "15.0";
        public const string Cdn = "source 'https://cdn.cocoapods.org/'";
        private const string Marker = "# cmpose: every Pod target on iOS " + MinimumIos;
        private static readonly Regex Source = new Regex(@"^\s*source\s+['""][^'""]*['""]\s*$");

        private static readonly string Hook = Marker + "\n" +
            "post_install do |installer|\n" +
            "  installer.pods_project.targets.each do |target|\n" +
            "    target.build_configurations.each do |config|\n" +
            "      config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '" + MinimumIos + "'\n" +
            "    end\n" +
            "  end\n" +
            "  installer.aggregate_targets.each do |aggregate|\n" +
            "    aggregate.user_build_configurations.each_key do |name|\n" +
            "      path = aggregate.xcconfig_path(name)\n" +
            "      next unless File.exist?(path)\n" +
            "      text = File.read(path)\n" +
            "      fixed = text.gsub(/^(OTHER_LDFLAGS = .*?) -framework \"GoogleMobileAds\"(.*)$/) { \"#{$1}#{$2} -framework \\\"GoogleMobileAds\\\"\" }\n" +
            "      File.write(path, fixed) if fixed != text\n" +
            "    end\n" +
            "  end\n" +
            "end\n";

        public static string Apply(string podfile)
        {
            var lines = podfile.Replace("\r\n", "\n").Split('\n').Where(l => !Source.IsMatch(l)).ToList();
            var body = string.Join("\n", lines).TrimEnd('\n') + "\n";
            if (!body.Contains(Marker))
            {
                if (body.Contains("post_install"))
                    throw new InvalidOperationException("The Podfile already has a post_install hook: add the iOS " + MinimumIos +
                        " deployment target for every Pod target to it, then remove the step in Assets/Editor/Build/PodfilePostProcessor.cs");
                body += "\n" + Hook;
            }
            return Cdn + "\n" + body;
        }
    }
}
