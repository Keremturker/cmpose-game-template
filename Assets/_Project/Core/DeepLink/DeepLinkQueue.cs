using System;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;

namespace Kturker.UnityBase.Core.DeepLink
{
    // Links that arrive before the menu has opened once (cold start URL, or a warm link during boot) wait here;
    // the latest one runs when the menu loads. After that every link runs at once.
    public sealed class DeepLinkQueue : IDisposable
    {
        private readonly DeepLinkRouter router;
        private readonly ISceneLoader loader;
        private readonly ILog log;
        private string pending;
        private bool ready;

        public DeepLinkQueue(DeepLinkRouter router, ISceneLoader loader, ILog log)
        {
            this.router = router;
            this.loader = loader;
            this.log = log;
            loader.Loaded += OnLoaded;
        }

        public void Receive(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            if (ready)
            {
                router.Route(url);
                return;
            }
            if (pending != null) log.Info($"Deep link replaced before the menu opened: {pending}");
            pending = url;
        }

        public void Dispose() => loader.Loaded -= OnLoaded;

        private void OnLoaded(SceneId id)
        {
            if (ready || id != SceneId.Menu) return;
            ready = true;
            var url = pending;
            pending = null;
            if (url != null) router.Route(url);
        }
    }
}
