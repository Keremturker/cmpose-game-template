using Cysharp.Threading.Tasks;
using Kturker.UnityBase.App.Update;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class UpdatePresenterTests
    {
        private const string Url = "https://example.com/min.json";
        private const string Lookup = "https://itunes.apple.com/lookup?bundleId=com.example.game";
        private const string StoreUrl = "https://apps.apple.com/app/id1";
        private const string Required = "{\"minVersion\":{\"android\":9,\"ios\":\"2.0.0\"}}";
        private const string Newer = "{\"resultCount\":1,\"results\":[{\"version\":\"1.1.0\",\"trackViewUrl\":\"https://apps.apple.com/app/id1\"}]}";

        private FakeHttpText http;
        private FakePlatformUpdater platform;
        private FakeUiFeedback feedback;
        private UpdateFlow flow;

        // A flow whose first menu load decides from the given answers (synchronously, unless pending)
        private void CreateFlow(StorePlatform store, string minJson = null, string lookupJson = null, bool pending = false)
        {
            http = new FakeHttpText();
            if (minJson != null) http.Responses[Url] = minJson;
            if (lookupJson != null) http.Responses[Lookup] = lookupJson;
            if (pending) http.Pending = new UniTaskCompletionSource<string>();
            platform = new FakePlatformUpdater { Platform = store, CurrentVersion = new AppVersion("1.0.0", 5) };
            feedback = new FakeUiFeedback();
            var log = new FakeLog();
            var loader = new FakeSceneLoader();
            var checker = new UpdateChecker(new FakePreferences(), http, platform, new FakeClock { NowMillis = 1_800_000_000_000L },
                new UpdateConfig(Url, "com.example.game"), log);
            flow = new UpdateFlow(loader, checker, platform, log);
            flow.Start();
            loader.LoadAsync(SceneId.Menu).Forget();
        }

        private UpdatePresenter Present(FakeUpdateView view)
        {
            var presenter = new UpdatePresenter(view, flow, platform, feedback);
            presenter.Start();
            return presenter;
        }

        [TearDown]
        public void End() => flow.Dispose();

        [Test]
        public void The_required_panel_has_no_way_to_close()
        {
            CreateFlow(StorePlatform.Android, Required);
            var view = new FakeUpdateView();
            Present(view);

            view.ClickLater();
            view.ClickLater();
            view.ClickUpdate();
            view.ClickUpdate();

            Assert.That(view.RequiredShown, Is.EqualTo(1));
            Assert.That(view.CanUpdate, Is.True);
            Assert.That(view.Hidden, Is.EqualTo(0));
            Assert.That(platform.Updates, Is.EqualTo(new string[] { null, null }));
            Assert.That(feedback.Clicks, Is.EqualTo(2));
            Assert.That(flow.Current.Kind, Is.EqualTo(UpdateKind.Required));
        }

        [Test]
        public void Without_an_app_store_page_the_required_panel_asks_to_update_from_the_app_store()
        {
            // Not on the App Store yet: the lookup has no result, so there is no page to open
            CreateFlow(StorePlatform.Ios, Required);
            platform.CanUpdate = false;
            var view = new FakeUpdateView();
            Present(view);

            Assert.That(view.RequiredShown, Is.EqualTo(1));
            Assert.That(view.CanUpdate, Is.False);
        }

        [Test]
        public void Update_on_the_available_panel_opens_the_store_and_closes_it()
        {
            CreateFlow(StorePlatform.Ios, lookupJson: Newer);
            var view = new FakeUpdateView();
            Present(view);

            view.ClickUpdate();

            Assert.That(view.AvailableShown, Is.EqualTo(1));
            Assert.That(platform.Updates, Is.EqualTo(new[] { StoreUrl }));
            Assert.That(view.Hidden, Is.EqualTo(1));
            Assert.That(flow.Current, Is.EqualTo(UpdateDecision.None));
        }

        [Test]
        public void Later_closes_the_available_panel_and_a_reloaded_menu_does_not_show_it_again()
        {
            CreateFlow(StorePlatform.Ios, lookupJson: Newer);
            var view = new FakeUpdateView();
            var presenter = Present(view);

            view.ClickLater();
            view.ClickLater();
            presenter.Dispose();
            var reloaded = new FakeUpdateView();
            Present(reloaded);

            Assert.That(view.Hidden, Is.EqualTo(1));
            Assert.That(feedback.Clicks, Is.EqualTo(1));
            Assert.That(platform.Updates, Is.Empty);
            Assert.That(reloaded.AvailableShown + reloaded.RequiredShown, Is.EqualTo(0));
        }

        [Test]
        public void A_late_decision_shows_the_panel_and_a_reloaded_menu_shows_a_standing_required_update()
        {
            CreateFlow(StorePlatform.Android, pending: true);
            var view = new FakeUpdateView();
            var presenter = Present(view);
            Assert.That(view.RequiredShown, Is.EqualTo(0));

            http.Pending.TrySetResult(Required);
            Assert.That(view.RequiredShown, Is.EqualTo(1));

            presenter.Dispose();
            var reloaded = new FakeUpdateView();
            Present(reloaded);

            Assert.That(reloaded.RequiredShown, Is.EqualTo(1));
            Assert.That(view.RequiredShown, Is.EqualTo(1));
        }
    }
}
