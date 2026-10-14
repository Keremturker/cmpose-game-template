using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Features.Shell.Ads;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Ads.Tests
{
    public sealed class MenuBannerTests
    {
        [Test]
        public void The_menu_shows_the_banner_and_every_other_scene_hides_it()
        {
            var loader = new FakeSceneLoader();
            var ads = new FakeAds();
            new MenuBanner(loader, ads).Start();

            loader.LoadAsync(SceneId.Menu);
            Assert.That(ads.BannerVisible, Is.True);
            loader.LoadAsync(SceneId.Game);

            Assert.That(ads.BannerVisible, Is.False);
            Assert.That(ads.BannerCalls, Is.EqualTo(new[] { "show", "hide" }));
        }

        [Test]
        public void After_dispose_scene_changes_do_nothing()
        {
            var loader = new FakeSceneLoader();
            var ads = new FakeAds();
            var banner = new MenuBanner(loader, ads);
            banner.Start();

            banner.Dispose();
            loader.LoadAsync(SceneId.Menu);

            Assert.That(ads.BannerCalls, Is.Empty);
        }
    }
}
