using UnityEngine;

namespace Kturker.UnityBase.Core.Network
{
    public sealed class UnityNetworkReachability : INetworkReachability
    {
        public bool IsOffline => Application.internetReachability == NetworkReachability.NotReachable;
    }
}
