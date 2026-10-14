using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Kturker.UnityBase.Core.Navigation
{
    public sealed class UnitySceneGateway : ISceneGateway
    {
        private readonly BuildScenes scenes;

        public UnitySceneGateway(BuildScenes scenes) => this.scenes = scenes;

        // The token is honoured until the load starts only: replacing the scene destroys the requesting scope and
        // cancels its token, which must not turn a finished load into a cancellation.
        public async UniTask LoadAsync(string sceneName, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (!scenes.Contains(sceneName)) throw NotInBuild(sceneName);
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null) throw NotInBuild(sceneName);
            await operation.ToUniTask();
        }

        private static InvalidOperationException NotInBuild(string sceneName) =>
            new InvalidOperationException($"Scene '{sceneName}' is not in Build Settings");
    }
}
