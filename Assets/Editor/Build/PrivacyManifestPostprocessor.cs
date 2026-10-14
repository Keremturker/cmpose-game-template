using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Kturker.UnityBase.Editor.Build
{
    // Copies Assets/Plugins/iOS/PrivacyInfo.xcprivacy into the app target's resources. The asset is excluded from
    // every platform (TemplateSetup.ConfigureNativePlugins), so Unity does not also put it beside UnityFramework's
    // own manifest; Xcode merges the app's and the framework's manifests.
    public sealed class PrivacyManifestPostprocessor : IPostprocessBuildWithReport
    {
        private const string Source = "Assets/Plugins/iOS/PrivacyInfo.xcprivacy";
        private const string FileName = "PrivacyInfo.xcprivacy";

        public int callbackOrder => 100;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;
            if (!File.Exists(Source)) throw new BuildFailedException($"{Source} is missing");
            var output = report.summary.outputPath;
            File.Copy(Source, Path.Combine(output, FileName), true);
            var projectPath = PBXProject.GetPBXProjectPath(output);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            if (project.FindFileGuidByProjectPath(FileName) == null)
            {
                var target = project.GetUnityMainTargetGuid();
                var file = project.AddFile(FileName, FileName, PBXSourceTree.Source);
                project.AddFileToBuildSection(target, project.GetResourcesBuildPhaseByTarget(target), file);
            }
            project.WriteToFile(projectPath);
            Debug.Log("CMPOSE_PRIVACY_MANIFEST added to the app target");
        }
    }
}
