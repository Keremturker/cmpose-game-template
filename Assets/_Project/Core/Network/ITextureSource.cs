using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kturker.UnityBase.Core.Network
{
    public interface ITextureSource
    {
        // null when the image could not be downloaded or decoded (or the call was cancelled)
        UniTask<Texture2D> DownloadAsync(string url, CancellationToken cancellation);
    }
}
