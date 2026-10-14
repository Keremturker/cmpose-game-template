using System;
using UnityEngine;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.DeepLink
{
    // Cold start: Application.absoluteURL (applied once the menu has loaded). Warm: Application.deepLinkActivated.
    public sealed class UnityDeepLinkSource : IStartable, IDisposable
    {
        private readonly DeepLinkQueue queue;

        public UnityDeepLinkSource(DeepLinkQueue queue) => this.queue = queue;

        public void Start()
        {
            Application.deepLinkActivated += queue.Receive;
            var url = Application.absoluteURL;
            if (!string.IsNullOrEmpty(url)) queue.Receive(url);
        }

        public void Dispose() => Application.deepLinkActivated -= queue.Receive;
    }
}
