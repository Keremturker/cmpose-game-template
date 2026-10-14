using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Kturker.UnityBase.Core.Network
{
    // UnityWebRequest with its own timeout (a linked token keeps a timeout and a caller's cancel apart); leaving the
    // using block disposes (aborts) an unfinished request. The URL is never logged here.
    public sealed class UnityWebRequestTransport : IHttpTransport
    {
        private static readonly IReadOnlyDictionary<string, string> NoHeaders = new Dictionary<string, string>();

        public async UniTask<HttpResponse> SendAsync(HttpRequest request, TimeSpan timeout, CancellationToken cancellation)
        {
            var clock = Stopwatch.StartNew();
            using var timer = new CancellationTokenSource(timeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, timer.Token);
            using var web = new UnityWebRequest(request.Url, request.Method, new DownloadHandlerBuffer(), null);
            foreach (var header in request.Headers) web.SetRequestHeader(header.Key, header.Value);
            _ = web.SendWebRequest();
            try
            {
                while (!web.isDone) await UniTask.Yield(PlayerLoopTiming.Update, linked.Token);
            }
            catch (OperationCanceledException)
            {
                var failure = cancellation.IsCancellationRequested ? TransportFailure.Cancelled : TransportFailure.Timeout;
                return new HttpResponse(failure, 0, null, NoHeaders, clock.Elapsed.TotalMilliseconds);
            }
            if (web.result == UnityWebRequest.Result.ConnectionError)
                return new HttpResponse(TransportFailure.Connection, 0, null, NoHeaders, clock.Elapsed.TotalMilliseconds);
            var headers = web.GetResponseHeaders();
            return new HttpResponse(TransportFailure.None, web.responseCode, web.downloadHandler.text,
                headers == null ? NoHeaders : (IReadOnlyDictionary<string, string>)headers, clock.Elapsed.TotalMilliseconds);
        }
    }
}
