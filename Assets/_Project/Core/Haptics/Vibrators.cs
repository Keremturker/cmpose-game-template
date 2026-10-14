using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Haptics
{
    public static class Vibrators
    {
        public static IVibrator ForPlatform(ILog log)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidVibrator(log);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosVibrator();
#else
            return new NullVibrator();
#endif
        }
    }
}
