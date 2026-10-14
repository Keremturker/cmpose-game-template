using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Kturker.UnityBase.Core.Leaderboards.Editor
{
    // iOS side of the leaderboards flag: GameKit for CmposeGameCenter.mm (compiled into UnityFramework) and the Game
    // Center capability (the com.apple.developer.game-center entitlement) on the app target. Runs after the ads
    // flag's post-processor (order 100); each reads and writes the Xcode project on its own.
    public sealed class LeaderboardsXcodePostProcessor : IPostprocessBuildWithReport
    {
        private const string Entitlements = "Game.entitlements";

        public int callbackOrder => 110;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;
            var projectPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            project.AddFrameworkToProject(project.GetUnityFrameworkTargetGuid(), "GameKit.framework", false);
            var mainTarget = project.GetUnityMainTargetGuid();
            project.WriteToFile(projectPath);
            var capabilities = new ProjectCapabilityManager(projectPath, Entitlements, null, mainTarget);
            capabilities.AddGameCenter();
            capabilities.WriteToFile();
            Debug.Log("CMPOSE_LEADERBOARDS_XCODE GameKit framework and Game Center entitlement added");
        }
    }
}
