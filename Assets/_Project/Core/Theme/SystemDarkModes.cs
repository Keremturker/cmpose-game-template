using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Theme
{
    public static class SystemDarkModes
    {
        public static ISystemDarkMode ForPlatform(ILog log)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidSystemDarkMode(log);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosSystemDarkMode();
#else
            return new NullSystemDarkMode();
#endif
        }
    }
}
