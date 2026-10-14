using System;
using Kturker.UnityBase.Core.DeepLink;
using Kturker.UnityBase.Core.Domain;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class DeepLinkRouterTests
    {
        [Test]
        public void Built_in_paths_open_their_scenes_and_print_the_marker()
        {
            var loader = new FakeSceneLoader();
            var log = new FakeLog();
            var router = new DeepLinkRouter(loader, TestGames.Game, log);

            Assert.That(router.Route("unitybase://game"), Is.True);
            Assert.That(router.Route("unitybase://menu"), Is.True);

            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Game, SceneId.Menu }));
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_DEEPLINK game"));
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_DEEPLINK menu"));
        }

        [Test]
        public void Paths_ignore_case_and_surrounding_slashes()
        {
            var loader = new FakeSceneLoader();
            var router = new DeepLinkRouter(loader, TestGames.Game, new FakeLog());

            Assert.That(router.Route("UnityBase://GAME/"), Is.True);
            Assert.That(router.Route("unitybase:///Menu"), Is.True);

            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Game, SceneId.Menu }));
        }

        [Test]
        public void A_feature_path_gets_the_query_until_its_registration_is_disposed()
        {
            var router = new DeepLinkRouter(new FakeSceneLoader(), TestGames.Game, new FakeLog());
            DeepLinkRequest received = null;
            var first = router.Register("Shop", request => received = request);

            Assert.That(router.Route("unitybase://shop/?item=gem%20pack&count=2&flag"), Is.True);
            Assert.That(received.Path, Is.EqualTo("shop"));
            Assert.That(received.Query["item"], Is.EqualTo("gem pack"));
            Assert.That(received.Query["count"], Is.EqualTo("2"));
            Assert.That(received.Query["flag"], Is.EqualTo(""));

            // A later registration replaces the first; disposing the stale one leaves it in place
            var calls = 0;
            var second = router.Register("shop", _ => calls++);
            first.Dispose();
            router.Route("unitybase://shop");
            Assert.That(calls, Is.EqualTo(1));

            second.Dispose();
            Assert.That(router.Route("unitybase://shop"), Is.False);
            Assert.Throws<ArgumentException>(() => router.Register("game", _ => { }));
        }

        [Test]
        public void An_unknown_path_is_logged_and_ignored()
        {
            var loader = new FakeSceneLoader();
            var log = new FakeLog();

            Assert.That(new DeepLinkRouter(loader, TestGames.Game, log).Route("unitybase://settings"), Is.False);

            Assert.That(loader.Requested, Is.Empty);
            Assert.That(log.Lines, Does.Contain("WARNING Deep link ignored, unknown path: settings"));
            Assert.That(log.Lines.FindAll(l => l.StartsWith("MARKER")), Is.Empty);
        }

        [Test]
        public void Text_that_is_not_a_link_is_logged_and_ignored()
        {
            var loader = new FakeSceneLoader();
            var log = new FakeLog();
            var router = new DeepLinkRouter(loader, TestGames.Game, log);

            foreach (var url in new[] { null, "", "   ", "game", "://game", "unitybase://", "unitybase://?x=1" })
                Assert.That(router.Route(url), Is.False, url ?? "null");

            Assert.That(loader.Requested, Is.Empty);
            Assert.That(log.Lines.FindAll(l => l.StartsWith("WARNING Deep link ignored, not a link")).Count, Is.EqualTo(7));
        }

        [Test]
        public void The_game_path_opens_the_registered_game()
        {
            var loader = new FakeSceneLoader();
            var router = new DeepLinkRouter(loader, new GameScene(SceneId.Menu, "other.play"), new FakeLog());

            Assert.That(router.Route("unitybase://game"), Is.True);

            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
        }
    }
}
