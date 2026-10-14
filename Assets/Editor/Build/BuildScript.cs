using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kturker.UnityBase.Editor.Build
{
    // Batchmode entry points: Unity -batchmode -nographics -projectPath <p> -buildTarget <t>
    //   -executeMethod Kturker.UnityBase.Editor.Build.BuildScript.<Method> -cmposeOut <path> [-cmposeSimulator] [-cmposeAab] [-cmposeDevelopment]
    // Every method ends the editor itself: 0 success, 1 build failed or threw, 2 bad input.
    // Android release signing comes from keystore.properties when it exists (AndroidSigning), for that build only.
    public static class BuildScript
    {
        private const int ExitOk = 0;
        private const int ExitFailed = 1;
        private const int ExitBadInput = 2;

        public static void BuildAndroid()
        {
            Run(BuildTarget.Android, () =>
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minifyRelease = true;
                EditorUserBuildSettings.buildAppBundle = HasFlag("-cmposeAab");
                return AndroidSigning.Apply(ProjectRoot);
            });
        }

        public static void BuildIOS()
        {
            Run(BuildTarget.iOS, () =>
            {
                var simulator = HasFlag("-cmposeSimulator");
                PlayerSettings.iOS.sdkVersion = simulator ? iOSSdkVersion.SimulatorSDK : iOSSdkVersion.DeviceSDK;
                // Unity defaults the simulator slice to x86_64, which Apple Silicon simulators refuse
                if (simulator) PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;
                return null;
            });
        }

        // Prints the identity Unity actually reads from ProjectSettings.asset, so a YAML rename can be checked
        public static void PrintIdentity()
        {
            Debug.Log("CMPOSE_IDENTITY"
                + $" company={PlayerSettings.companyName}"
                + $" product={PlayerSettings.productName}"
                + $" version={PlayerSettings.bundleVersion}"
                + $" android={PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)}"
                + $" ios={PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS)}"
                + $" androidMinSdk={(int)PlayerSettings.Android.minSdkVersion}"
                + $" iosMin={PlayerSettings.iOS.targetOSVersionString}");
            EditorApplication.Exit(ExitOk);
        }

        private static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);

        // configure applies per-build settings and returns the action that undoes the temporary ones (or null);
        // it runs before the editor exits, so ProjectSettings.asset keeps its committed values
        private static void Run(BuildTarget target, Func<Action> configure)
        {
            Action restore = null;
            int exitCode;
            try
            {
                var output = ArgValue("-cmposeOut");
                if (string.IsNullOrEmpty(output)) throw new BuildInputException("missing -cmposeOut");
                var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
                if (scenes.Length == 0) throw new BuildInputException("no enabled scenes in Build Settings");
                restore = configure();
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = output,
                    target = target,
                    // -cmposeDevelopment: a development build (Debug.isDebugBuild, the network inspector, the debug base URL)
                    options = HasFlag("-cmposeDevelopment") ? BuildOptions.Development : BuildOptions.None
                });
                var summary = report.summary;
                Debug.Log($"CMPOSE_BUILD_RESULT target={target} result={summary.result} errors={summary.totalErrors}"
                    + $" warnings={summary.totalWarnings} seconds={(int)summary.totalTime.TotalSeconds} out={output}");
                exitCode = summary.result == BuildResult.Succeeded ? ExitOk : ExitFailed;
            }
            catch (BuildInputException e)
            {
                Debug.LogError($"CMPOSE_BUILD_RESULT result=BadInput reason={e.Message}");
                exitCode = ExitBadInput;
            }
            catch (Exception e)
            {
                Debug.LogError($"CMPOSE_BUILD_RESULT target={target} result=Exception {e}");
                exitCode = ExitFailed;
            }
            finally
            {
                if (restore != null)
                {
                    restore();
                    AssetDatabase.SaveAssets();
                }
            }
            EditorApplication.Exit(exitCode);
        }

        private static string ArgValue(string name)
        {
            var args = Environment.GetCommandLineArgs();
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        private static bool HasFlag(string name) => Environment.GetCommandLineArgs().Contains(name);
    }
}
