using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Network
{
    public interface IHttpTransport
    {
        // Network problems come back as HttpResponse.Failure, never as exceptions
        UniTask<HttpResponse> SendAsync(HttpRequest request, TimeSpan timeout, CancellationToken cancellation);
    }
}
