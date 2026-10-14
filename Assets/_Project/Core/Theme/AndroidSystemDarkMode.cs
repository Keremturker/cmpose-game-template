#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;

namespace Kturker.UnityBase.Core.Theme
{
    // Configuration.uiMode & UI_MODE_NIGHT_MASK of the activity's resources; light after an error
    public sealed class AndroidSystemDarkMode : ISystemDarkMode
    {
        private const int NightMask = 0x30;
        private const int NightYes = 0x20;
        private readonly ILog log;
        private bool failed;

        public AndroidSystemDarkMode(ILog log) => this.log = log;

        public bool IsDark
        {
            get
            {
                if (failed) return false;
                try
                {
                    using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    using var resources = activity.Call<AndroidJavaObject>("getResources");
                    using var configuration = resources.Call<AndroidJavaObject>("getConfiguration");
                    return (configuration.Get<int>("uiMode") & NightMask) == NightYes;
                }
                catch (Exception e)
                {
                    failed = true;
                    log.Warning($"System dark mode unavailable: {e.Message}");
                    return false;
                }
            }
        }
    }
}
#endif
