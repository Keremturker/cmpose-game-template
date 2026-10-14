using System.Collections;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.DeepLink;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class DeepLinkQueueTests
    {
        [UnityTest]
        public IEnumerator A_link_before_the_menu_waits_and_runs_once_after_it_loads() => UniTask.ToCoroutine(async () =>
        {
            var gateway = new FakeGateway();
            var log = new FakeLog();
            var loader = new SceneLoader(gateway, new FakeOverlay(), log);
            var router = new DeepLinkRouter(loader, TestGames.Game, log);
            using var queue = new DeepLinkQueue(router, loader, log);

            // Cold start URL, then a warm link while boot is still running: the later one wins
            queue.Receive("unitybase://menu");
            queue.Receive("unitybase://game");
            Assert.That(gateway.Loaded, Is.Empty);

            // What BootEntry does at the end of boot
            await loader.LoadAsync(SceneId.Menu);

            Assert.That(gateway.Loaded, Is.EqualTo(new[] { "Menu", "Game" }));
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_DEEPLINK game"));
            Assert.That(log.Lines, Does.Not.Contain("MARKER CMPOSE_DEEPLINK menu"));

            await loader.LoadAsync(SceneId.Menu);
            Assert.That(gateway.Loaded, Is.EqualTo(new[] { "Menu", "Game", "Menu" }));
        });

        [Test]
        public void A_link_after_the_menu_opened_runs_at_once()
        {
            var loader = new FakeSceneLoader();
            var log = new FakeLog();
            using var queue = new DeepLinkQueue(new DeepLinkRouter(loader, TestGames.Game, log), loader, log);
            loader.LoadAsync(SceneId.Menu).Forget();

            queue.Receive("unitybase://game");
            queue.Receive(null);
            queue.Receive("");

            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu, SceneId.Game }));
        }
    }
}
