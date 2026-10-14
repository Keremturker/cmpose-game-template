using System.Collections.Generic;

namespace Kturker.UnityBase.Core.Network
{
    public sealed class HttpResponse
    {
        public HttpResponse(TransportFailure failure, long statusCode, string body, IReadOnlyDictionary<string, string> headers, double milliseconds)
        {
            Failure = failure;
            StatusCode = statusCode;
            Body = body;
            Headers = headers ?? new Dictionary<string, string>();
            Milliseconds = milliseconds;
        }

        public TransportFailure Failure { get; }
        public long StatusCode { get; }
        public string Body { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public double Milliseconds { get; }
    }
}
