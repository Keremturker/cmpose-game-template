using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;

namespace Kturker.UnityBase.Core.Network
{
    // Memory cache by URL, least recently used first out; a texture that leaves the cache is released (destroyed).
    // Failures are not cached, so the next call tries again. URLs are not logged.
    public sealed class RemoteImageLoader : IRemoteImageLoader
    {
        public const int DefaultCapacity = 32;

        private readonly ITextureSource source;
        private readonly ILog log;
        private readonly Action<Texture2D> release;
        private readonly int capacity;
        private readonly LinkedList<KeyValuePair<string, Texture2D>> recent = new LinkedList<KeyValuePair<string, Texture2D>>();
        private readonly Dictionary<string, LinkedListNode<KeyValuePair<string, Texture2D>>> byUrl =
            new Dictionary<string, LinkedListNode<KeyValuePair<string, Texture2D>>>(StringComparer.Ordinal);

        public RemoteImageLoader(ITextureSource source, ILog log, Action<Texture2D> release, int capacity = DefaultCapacity)
        {
            this.source = source;
            this.log = log;
            this.release = release;
            this.capacity = capacity;
        }

        public async UniTask<Texture2D> LoadAsync(string url, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (TryTake(url, out var cached)) return cached;
            var texture = await source.DownloadAsync(url, cancellation);
            if (texture == null)
            {
                if (!cancellation.IsCancellationRequested) log.Warning("Remote image not loaded");
                return null;
            }
            // A parallel load of the same URL finished first: keep that one
            if (TryTake(url, out cached))
            {
                release(texture);
                return cached;
            }
            byUrl[url] = recent.AddFirst(new KeyValuePair<string, Texture2D>(url, texture));
            while (recent.Count > capacity)
            {
                var oldest = recent.Last.Value;
                recent.RemoveLast();
                byUrl.Remove(oldest.Key);
                release(oldest.Value);
            }
            return texture;
        }

        private bool TryTake(string url, out Texture2D texture)
        {
            if (!byUrl.TryGetValue(url, out var node))
            {
                texture = null;
                return false;
            }
            recent.Remove(node);
            recent.AddFirst(node);
            texture = node.Value.Value;
            return true;
        }
    }
}
