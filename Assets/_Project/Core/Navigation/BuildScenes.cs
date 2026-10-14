using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Kturker.UnityBase.Core.Navigation
{
    // Names of the scenes in Build Settings, given to UnitySceneGateway so a missing scene fails loudly
    // (SceneManager.LoadSceneAsync only logs and returns null) and so the check runs in EditMode tests
    public sealed class BuildScenes
    {
        private readonly HashSet<string> names;

        public BuildScenes(IEnumerable<string> names) => this.names = new HashSet<string>(names, StringComparer.Ordinal);

        public bool Contains(string sceneName) => names.Contains(sceneName);

        public static BuildScenes FromPaths(IEnumerable<string> paths) => new BuildScenes(paths.Select(Path.GetFileNameWithoutExtension));

        public static BuildScenes FromBuildSettings()
        {
            var paths = new List<string>();
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) paths.Add(SceneUtility.GetScenePathByBuildIndex(i));
            return FromPaths(paths);
        }
    }
}
