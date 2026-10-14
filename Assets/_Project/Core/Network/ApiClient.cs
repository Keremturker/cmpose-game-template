using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Newtonsoft.Json;

namespace Kturker.UnityBase.Core.Network
{
    // GET + JSON against the base URL. Every outcome is an ApiResult; the log gets the path (no query string, no
    // headers) and the status only.
    public sealed class ApiClient : IApiClient
    {
        private static readonly IReadOnlyDictionary<string, string> JsonHeaders =
            new Dictionary<string, string> { { "Accept", "application/json" } };

        private readonly IHttpTransport transport;
        private readonly string baseUrl;
        private readonly IReadOnlyList<IRequestObserver> observers;
        private readonly INetworkReachability reachability;
        private readonly ILog log;

        public ApiClient(IHttpTransport transport, string baseUrl, IReadOnlyList<IRequestObserver> observers,
            INetworkReachability reachability, ILog log)
        {
            this.transport = transport;
            this.baseUrl = baseUrl;
            this.observers = observers;
            this.reachability = reachability;
            this.log = log;
        }

        public static string Combine(string baseUrl, string path) => $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

        public async UniTask<ApiResult<T>> GetJson<T>(string path, CancellationToken cancellation) where T : class
        {
            var logPath = path.Split('?')[0].TrimStart('/');
            if (cancellation.IsCancellationRequested) return ApiResult<T>.Failure(new NetworkError(NetworkErrorKind.Cancelled));
            if (reachability.IsOffline)
            {
                log.Debug($"HTTP GET {logPath} -> offline");
                return ApiResult<T>.Failure(new NetworkError(NetworkErrorKind.NoConnection));
            }
            var request = new HttpRequest("GET", Combine(baseUrl, path), JsonHeaders);
            var response = await transport.SendAsync(request, ApiConfig.Timeout, cancellation);
            Notify(request, response);
            var status = response.Failure == TransportFailure.None
                ? response.StatusCode.ToString(CultureInfo.InvariantCulture)
                : response.Failure.ToString();
            log.Debug($"HTTP GET {logPath} -> {status}");
            switch (response.Failure)
            {
                case TransportFailure.Connection: return ApiResult<T>.Failure(new NetworkError(NetworkErrorKind.NoConnection));
                case TransportFailure.Timeout: return ApiResult<T>.Failure(new NetworkError(NetworkErrorKind.Timeout));
                case TransportFailure.Cancelled: return ApiResult<T>.Failure(new NetworkError(NetworkErrorKind.Cancelled));
            }
            if (response.StatusCode < 200 || response.StatusCode > 299)
                return ApiResult<T>.Failure(NetworkError.Http(response.StatusCode, response.Body));
            T value;
            try
            {
                value = JsonConvert.DeserializeObject<T>(response.Body ?? string.Empty);
            }
            catch (JsonException e)
            {
                log.Warning($"HTTP GET {logPath}: unreadable JSON ({e.GetType().Name})");
                return ApiResult<T>.Failure(new NetworkError(NetworkErrorKind.Parse));
            }
            if (value == null)
            {
                log.Warning($"HTTP GET {logPath}: empty JSON");
                return ApiResult<T>.Failure(new NetworkError(NetworkErrorKind.Parse));
            }
            return ApiResult<T>.Success(value);
        }

        private void Notify(HttpRequest request, HttpResponse response)
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnCompleted(request, response);
                }
                catch (Exception e)
                {
                    log.Warning($"Request observer {observer.GetType().Name} failed: {e.Message}");
                }
            }
        }
    }
}
