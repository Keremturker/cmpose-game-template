using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Kturker.UnityBase.Core.Ads.Editor
{
    // iOS side of the ads flag: the App Tracking Transparency prompt text (AdsConfig.TrackingUsageDescription) and the
    // framework CmposeTracking.mm needs. Google Mobile Ads' own post-processor writes GADApplicationIdentifier and the
    // SKAdNetwork items from GoogleMobileAdsSettings; a build without the App ID fails here instead of crashing at launch.
    public sealed class AdsXcodePostProcessor : IPostprocessBuildWithReport
    {
        private const string AppIdKey = "GADApplicationIdentifier";
        private const string TrackingKey = "NSUserTrackingUsageDescription";

        public int callbackOrder => 100;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;
            var output = report.summary.outputPath;
            var plistPath = Path.Combine(output, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            if (!plist.root.values.ContainsKey(AppIdKey))
                throw new BuildFailedException($"Info.plist has no {AppIdKey}: set adMobIOSAppId in Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            plist.root.SetString(TrackingKey, AdsConfig.TrackingUsageDescription);
            plist.WriteToFile(plistPath);
            var projectPath = PBXProject.GetPBXProjectPath(output);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            project.AddFrameworkToProject(project.GetUnityFrameworkTargetGuid(), "AppTrackingTransparency.framework", false);
            project.WriteToFile(projectPath);
            Debug.Log("CMPOSE_ADS_XCODE tracking prompt and framework added");
        }
    }
}
