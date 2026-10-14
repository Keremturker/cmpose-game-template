#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kturker.UnityBase.Core.Review
{
    // Google Play In-App Review through Assets/Plugins/Android/CmposePlay.androidlib (dev.cmpose.play.CmposeReview).
    // Play shows the dialog only to an app installed from Google Play and limits how often; elsewhere the flow ends
    // without a dialog or with an error, which is logged. Review requests turn themselves off after a bridge error.
    public sealed class PlayReviewRequester : IReviewRequester
    {
        private const string Bridge = "dev.cmpose.play.CmposeReview";
        private readonly ILog log;
        private bool failed;

        public PlayReviewRequester(ILog log) => this.log = log;

        public void RequestReview()
        {
            if (failed) return;
            try
            {
                using var bridge = new AndroidJavaClass(Bridge);
                using var activity = CurrentActivity();
                bridge.CallStatic("requestReview", activity, new Callback(log));
            }
            catch (Exception e)
            {
                failed = true;
                log.Warning($"In-app review turned off: {e.Message}");
            }
        }

        public void OpenStorePage(string url, string fallbackUrl)
        {
            try
            {
                using var bridge = new AndroidJavaClass(Bridge);
                using var activity = CurrentActivity();
                // JNI cannot type a null argument, so "no fallback" is an empty string
                if (!bridge.CallStatic<bool>("openUrl", activity, url, fallbackUrl ?? string.Empty)) log.Warning("No app can open the Play Store page");
            }
            catch (Exception e)
            {
                log.Warning($"Could not open the Play Store page: {e.Message}");
            }
        }

        private static AndroidJavaObject CurrentActivity()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return player.GetStatic<AndroidJavaObject>("currentActivity");
        }

        // dev.cmpose.play.CmposeCallback; Play calls it on the Android UI thread, so it only logs
        private sealed class Callback : AndroidJavaProxy
        {
            private readonly ILog log;

            public Callback(ILog log) : base("dev.cmpose.play.CmposeCallback") => this.log = log;

            [Preserve]
            public void onResult(bool ok, string detail)
            {
                if (ok) log.Info("Review flow finished");
                else log.Warning($"Review flow unavailable: {detail}");
            }
        }
    }
}
#endif
