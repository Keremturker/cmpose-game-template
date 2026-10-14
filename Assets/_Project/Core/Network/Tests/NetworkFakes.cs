using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kturker.UnityBase.Core.Network.Tests
{
    public sealed class FakeTransport : IHttpTransport
    {
        private static readonly IReadOnlyDictionary<string, string> NoHeaders = new Dictionary<string, string>();
        public readonly List<HttpRequest> Requests = new List<HttpRequest>();
        public readonly List<TimeSpan> Timeouts = new List<TimeSpan>();
        public HttpResponse Response;

        public static HttpResponse Ok(string body) => Status(200, body);

        public static HttpResponse Status(long code, string body) => new HttpResponse(TransportFailure.None, code, body,
            new Dictionary<string, string> { { "Content-Type", "application/json" } }, 12);

        public static HttpResponse Failed(TransportFailure failure) => new HttpResponse(failure, 0, null, NoHeaders, 15000);

        public UniTask<HttpResponse> SendAsync(HttpRequest request, TimeSpan timeout, CancellationToken cancellation)
        {
            Requests.Add(request);
            Timeouts.Add(timeout);
            return UniTask.FromResult(Response);
        }
    }

    public sealed class FakeReachability : INetworkReachability
    {
        public bool IsOffline { get; set; }
    }

    public sealed class RecordingObserver : IRequestObserver
    {
        public readonly List<HttpRequest> Requests = new List<HttpRequest>();
        public readonly List<HttpResponse> Responses = new List<HttpResponse>();

        public void OnCompleted(HttpRequest request, HttpResponse response)
        {
            Requests.Add(request);
            Responses.Add(response);
        }
    }

    public sealed class ThrowingObserver : IRequestObserver
    {
        public void OnCompleted(HttpRequest request, HttpResponse response) => throw new InvalidOperationException("boom");
    }

    public sealed class FakeApiClient : IApiClient
    {
        public readonly List<string> Paths = new List<string>();
        public readonly Queue<object> Results = new Queue<object>();
        // When set, the next call waits for it (the presenter's request still running)
        public UniTaskCompletionSource<object> Pending;

        public async UniTask<ApiResult<T>> GetJson<T>(string path, CancellationToken cancellation) where T : class
        {
            Paths.Add(path);
            if (Pending == null) return (ApiResult<T>)Results.Dequeue();
            var pending = Pending;
            Pending = null;
            return (ApiResult<T>)await pending.Task;
        }
    }

    public sealed class FakeImageLoader : IRemoteImageLoader
    {
        public readonly List<string> Urls = new List<string>();
        public Texture2D Texture;

        public UniTask<Texture2D> LoadAsync(string url, CancellationToken cancellation)
        {
            Urls.Add(url);
            return UniTask.FromResult(string.IsNullOrEmpty(url) ? null : Texture);
        }
    }
}
