using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;

namespace Kturker.UnityBase.Core.DeepLink
{
    // Maps a link's path to a handler: 'menu' opens the menu, 'game' the registered game (GameScene); features add paths with Register.
    // Unknown paths and text that is not a link are logged as warnings and ignored.
    public sealed class DeepLinkRouter
    {
        private readonly Dictionary<string, Action<DeepLinkRequest>> routes = new Dictionary<string, Action<DeepLinkRequest>>(StringComparer.Ordinal);
        private readonly ILog log;

        public DeepLinkRouter(ISceneLoader loader, GameScene game, ILog log)
        {
            this.log = log;
            routes["menu"] = _ => loader.LoadAsync(SceneId.Menu).Forget();
            routes["game"] = _ => loader.LoadAsync(game.Scene).Forget();
        }

        // A later registration of the same path replaces the earlier one. Dispose the result when the feature's
        // scope ends; disposing a replaced registration changes nothing.
        public IDisposable Register(string path, Action<DeepLinkRequest> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var key = (path ?? string.Empty).Trim('/').ToLowerInvariant();
            if (key.Length == 0 || key == "menu" || key == "game")
                throw new ArgumentException($"Deep link path '{path}' is empty or built in", nameof(path));
            routes[key] = handler;
            return new Registration(routes, key, handler);
        }

        public bool Route(string url)
        {
            if (!DeepLinkRequest.TryParse(url, out var request))
            {
                log.Warning($"Deep link ignored, not a link: {url}");
                return false;
            }
            if (!routes.TryGetValue(request.Path, out var handler))
            {
                log.Warning($"Deep link ignored, unknown path: {request.Path}");
                return false;
            }
            log.Marker($"CMPOSE_DEEPLINK {request.Path}");
            handler(request);
            return true;
        }

        private sealed class Registration : IDisposable
        {
            private readonly Dictionary<string, Action<DeepLinkRequest>> routes;
            private readonly string key;
            private readonly Action<DeepLinkRequest> handler;

            public Registration(Dictionary<string, Action<DeepLinkRequest>> routes, string key, Action<DeepLinkRequest> handler)
            {
                this.routes = routes;
                this.key = key;
                this.handler = handler;
            }

            public void Dispose()
            {
                if (routes.TryGetValue(key, out var current) && current == handler) routes.Remove(key);
            }
        }
    }
}
