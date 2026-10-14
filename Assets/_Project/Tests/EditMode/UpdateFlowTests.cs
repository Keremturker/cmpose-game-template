using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class UpdateFlowTests
    {
        private const string Url = "https://example.com/min.json";
        private const string Required = "{\"minVersion\":{\"android\":9}}";

        private readonly List<UpdateDecision> decided = new List<UpdateDecision>();
        private FakePreferences prefs;
        private FakeHttpText http;
        private FakePlatformUpdater platform;
        private FakeSceneLoader loader;
        private FakeLog log;
        private UpdateFlow flow;

        private void Create(StorePlatform store = StorePlatform.Android, string minVersionUrl = Url)
        {
            prefs = new FakePreferences();
            http = new FakeHttpText();
            platform = new FakePlatformUpdater { Platform = store, CurrentVersion = new AppVersion("1.0.0", 5) };
            loader = new FakeSceneLoader();
            log = new FakeLog();
            decided.Clear();
            var checker = new UpdateChecker(prefs, http, platform, new FakeClock { NowMillis = 1_800_000_000_000L },
                new UpdateConfig(minVersionUrl, "com.example.game"), log);
            flow = new UpdateFlow(loader, checker, platform, log);
            flow.Decided += decided.Add;
            flow.Start();
        }

        [TearDown]
        public void End()
        {
            if (flow != null) flow.Dispose();
        }

        [Test]
        public void Only_the_first_menu_load_checks()
        {
            Create();
            http.Responses[Url] = "{\"minVersion\":{\"android\":3}}";

            loader.LoadAsync(SceneId.Menu).Forget();
            loader.LoadAsync(SceneId.Game).Forget();
            loader.LoadAsync(SceneId.Menu).Forget();

            Assert.That(http.Requested, Is.EqualTo(new[] { Url }));
            Assert.That(flow.Current, Is.EqualTo(UpdateDecision.None));
            Assert.That(decided, Is.EqualTo(new[] { UpdateDecision.None }));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu, SceneId.Game, SceneId.Menu }));
        }

        [Test]
        public void A_required_update_decided_away_from_the_menu_sends_the_player_back_every_time()
        {
            Create();
            http.Pending = new UniTaskCompletionSource<string>();
            loader.LoadAsync(SceneId.Menu).Forget();
            // Play pressed, or a cold deep link to game, before the answer arrives
            loader.LoadAsync(SceneId.Game).Forget();

            http.Pending.TrySetResult(Required);

            Assert.That(flow.Current, Is.EqualTo(UpdateDecision.Required(null)));
            Assert.That(decided, Is.EqualTo(new[] { UpdateDecision.Required(null) }));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu, SceneId.Game, SceneId.Menu }));

            loader.LoadAsync(SceneId.Game).Forget();

            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu, SceneId.Game, SceneId.Menu, SceneId.Game, SceneId.Menu }));
            Assert.That(log.Lines, Does.Contain("WARNING Update required: back to the menu"));
            Assert.That(platform.FlexibleRequests, Is.EqualTo(0));
        }

        [Test]
        public void With_nothing_required_android_offers_the_play_flexible_update_and_ios_does_not()
        {
            Create();
            platform.FlexibleOffered = true;
            loader.LoadAsync(SceneId.Menu).Forget();

            Assert.That(platform.FlexibleRequests, Is.EqualTo(1));
            Assert.That(log.Lines, Does.Contain("INFO Google Play flexible update started"));
            flow.Dispose();

            Create(StorePlatform.Ios, minVersionUrl: "");
            platform.FlexibleOffered = true;
            loader.LoadAsync(SceneId.Menu).Forget();

            Assert.That(flow.Current, Is.EqualTo(UpdateDecision.None));
            Assert.That(platform.FlexibleRequests, Is.EqualTo(0));
        }

        [Test]
        public void A_failing_check_or_quitting_during_the_check_stays_silent()
        {
            Create();
            platform.VersionFailure = new InvalidOperationException("no package info");
            loader.LoadAsync(SceneId.Menu).Forget();

            Assert.That(flow.Current, Is.Null);
            Assert.That(decided, Is.Empty);
            Assert.That(log.Lines, Does.Contain("WARNING Update check failed: no package info"));
            flow.Dispose();

            Create();
            http.Pending = new UniTaskCompletionSource<string>();
            loader.LoadAsync(SceneId.Menu).Forget();
            flow.Dispose();
            http.Pending.TrySetResult(Required);

            Assert.That(flow.Current, Is.Null);
            Assert.That(decided, Is.Empty);
            Assert.That(prefs.Values.ContainsKey(UpdateChecker.LastCheckKey), Is.False);
            loader.LoadAsync(SceneId.Game).Forget();
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu, SceneId.Game }));
        }
    }
}
