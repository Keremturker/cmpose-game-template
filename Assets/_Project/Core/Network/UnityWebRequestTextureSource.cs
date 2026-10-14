using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Kturker.UnityBase.Core.Network
{
    // UnityWebRequestTexture with ApiConfig's timeout; leaving the using block aborts an unfinished request
    public sealed class UnityWebRequestTextureSource : ITextureSource
    {
        public async UniTask<Texture2D> DownloadAsync(string url, CancellationToken cancellation)
        {
            using var timer = new CancellationTokenSource(ApiConfig.Timeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, timer.Token);
            using var web = UnityWebRequestTexture.GetTexture(url);
            _ = web.SendWebRequest();
            try
            {
                while (!web.isDone) await UniTask.Yield(PlayerLoopTiming.Update, linked.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            return web.result == UnityWebRequest.Result.Success ? DownloadHandlerTexture.GetContent(web) : null;
        }
    }
}
