using System;
using Kturker.UnityBase.Core.Ads;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Shell.Ads
{
    // The menu's banner: at the bottom of the menu, hidden in every other scene (SafeArea keeps the menu above it)
    public sealed class MenuBanner : IStartable, IDisposable
    {
        private readonly ISceneLoader loader;
        private readonly IAds ads;

        public MenuBanner(ISceneLoader loader, IAds ads)
        {
            this.loader = loader;
            this.ads = ads;
        }

        public void Start() => loader.Loaded += OnLoaded;

        public void Dispose() => loader.Loaded -= OnLoaded;

        private void OnLoaded(SceneId scene)
        {
            if (scene == SceneId.Menu) ads.ShowBanner();
            else ads.HideBanner();
        }
    }
}
