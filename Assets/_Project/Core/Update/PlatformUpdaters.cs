using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Update
{
    public static class PlatformUpdaters
    {
        public static IPlatformUpdater ForPlatform(ILog log)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new PlayPlatformUpdater(log);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosPlatformUpdater(log);
#else
            return new NullPlatformUpdater(log);
#endif
        }
    }
}
