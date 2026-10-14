using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using UnityEngine.Networking;

namespace Kturker.UnityBase.Core.Update
{
    // UnityWebRequest with a 5 s timeout; cancelling disposes (aborts) the request. The URL is never logged: it may be
    // a private address.
    public sealed class UnityWebRequestHttpText : IHttpText
    {
        public const int TimeoutSeconds = 5;
        private readonly ILog log;

        public UnityWebRequestHttpText(ILog log) => this.log = log;

        public async UniTask<string> GetAsync(string url, CancellationToken cancellation)
        {
            using var request = UnityWebRequest.Get(url);
            request.timeout = TimeoutSeconds;
            _ = request.SendWebRequest();
            while (!request.isDone) await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200) return request.downloadHandler.text;
            log.Warning($"Update check request failed: {request.result} {request.responseCode}");
            return null;
        }
    }
}
