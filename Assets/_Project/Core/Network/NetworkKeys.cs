using System;

namespace Kturker.UnityBase.Core.Network
{
    // Keys of Resources/Strings/<language>/network.json
    public static class NetworkKeys
    {
        public const string Loading = "network.loading";
        public const string Retry = "network.retry";

        public static string Error(NetworkErrorKind kind)
        {
            switch (kind)
            {
                case NetworkErrorKind.NoConnection: return "network.error.noConnection";
                case NetworkErrorKind.Timeout: return "network.error.timeout";
                case NetworkErrorKind.Http: return "network.error.http";
                case NetworkErrorKind.Parse: return "network.error.parse";
                case NetworkErrorKind.Cancelled: return "network.error.cancelled";
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
    }
}
