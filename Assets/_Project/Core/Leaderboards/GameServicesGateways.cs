using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Leaderboards
{
    public static class GameServicesGateways
    {
        public static IGameServicesGateway Create(bool configured, GameIds ids, ILog log)
        {
            if (!configured) return new NoGameServicesGateway();
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidGameServicesGateway(ids, log);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosGameServicesGateway(ids, log);
#else
            return new NoGameServicesGateway();
#endif
        }
    }
}
