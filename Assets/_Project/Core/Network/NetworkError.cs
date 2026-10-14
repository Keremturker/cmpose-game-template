namespace Kturker.UnityBase.Core.Network
{
    // Http carries the status code and the start of the body (for messages, never shown raw to players)
    public sealed class NetworkError
    {
        public const int BodyLimit = 512;

        public NetworkError(NetworkErrorKind kind) : this(kind, 0, null)
        {
        }

        private NetworkError(NetworkErrorKind kind, long statusCode, string body)
        {
            Kind = kind;
            StatusCode = statusCode;
            Body = body;
        }

        public NetworkErrorKind Kind { get; }
        public long StatusCode { get; }
        public string Body { get; }

        public static NetworkError Http(long statusCode, string body) =>
            new NetworkError(NetworkErrorKind.Http, statusCode, body != null && body.Length > BodyLimit ? body.Substring(0, BodyLimit) : body);

        public override string ToString() => Kind == NetworkErrorKind.Http ? $"Http {StatusCode}" : Kind.ToString();
    }
}
