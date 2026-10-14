using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kturker.UnityBase.Core.Network
{
    public interface IRemoteImageLoader
    {
        // null when the image could not be loaded. The loader owns the texture: callers show it, never destroy it.
        UniTask<Texture2D> LoadAsync(string url, CancellationToken cancellation);
    }
}
