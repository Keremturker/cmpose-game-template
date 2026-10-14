using System.Collections.Generic;

namespace Kturker.UnityBase.Core.Network
{
    public sealed class HttpRequest
    {
        public HttpRequest(string method, string url, IReadOnlyDictionary<string, string> headers)
        {
            Method = method;
            Url = url;
            Headers = headers ?? new Dictionary<string, string>();
        }

        public string Method { get; }
        public string Url { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
    }
}
