using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Navigation;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class UnitySceneGatewayTests
    {
        [UnityTest]
        public IEnumerator A_scene_missing_from_build_settings_fails_with_its_name() => UniTask.ToCoroutine(async () =>
        {
            var scenes = BuildScenes.FromPaths(new[] { "Assets/_Project/Scenes/Boot.unity", "Assets/_Project/Scenes/Menu.unity" });
            Assert.That(scenes.Contains("Menu"), Is.True);
            Assert.That(scenes.Contains("Game"), Is.False);

            InvalidOperationException thrown = null;
            try { await new UnitySceneGateway(scenes).LoadAsync("Game", CancellationToken.None); }
            catch (InvalidOperationException e) { thrown = e; }

            Assert.That(thrown, Is.Not.Null);
            Assert.That(thrown.Message, Is.EqualTo("Scene 'Game' is not in Build Settings"));
        });
    }
}
