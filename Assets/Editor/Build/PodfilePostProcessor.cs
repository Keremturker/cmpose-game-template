using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Kturker.UnityBase.Editor.Build
{
    // Between EDM4U's Podfile generation (callback order 40) and its pod install (50). Only flags with iOS pods
    // (purchases, ads) make EDM4U write a Podfile; without one there is nothing to patch.
    public static class PodfilePostProcessor
    {
        [PostProcessBuild(45)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;
            var podfile = Path.Combine(path, "Podfile");
            if (!File.Exists(podfile)) return;
            try
            {
                File.WriteAllText(podfile, PodfilePatch.Apply(File.ReadAllText(podfile)));
            }
            catch (System.InvalidOperationException e)
            {
                throw new BuildFailedException(e.Message);
            }
            Debug.Log("CMPOSE_PODFILE patched");
        }
    }
}
