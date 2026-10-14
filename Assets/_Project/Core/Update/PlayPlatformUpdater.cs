#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kturker.UnityBase.Core.Update
{
    // Google Play In-App Updates through Assets/Plugins/Android/CmposePlay.androidlib (dev.cmpose.play.CmposeAppUpdate).
    // Play offers updates only to an app installed from Google Play; elsewhere the request fails, which is logged and
    // treated as "no update". The immediate flow falls back to the Play Store page.
    public sealed class PlayPlatformUpdater : IPlatformUpdater
    {
        private const string Bridge = "dev.cmpose.play.CmposeAppUpdate";
        private readonly ILog log;
        private AppVersion version;

        public PlayPlatformUpdater(ILog log) => this.log = log;

        public StorePlatform Platform => StorePlatform.Android;

        public AppVersion CurrentVersion => version ??= new AppVersion(Application.version, ReadVersionCode());

        public UniTask<bool> StartFlexibleUpdateAsync(CancellationToken cancellation) => StartAsync(false, cancellation);

        public bool CanStartUpdate(string storeUrl) => true;

        public void StartUpdate(string storeUrl) => StartImmediateAsync().Forget();

        private async UniTaskVoid StartImmediateAsync()
        {
            if (await StartAsync(true, CancellationToken.None)) return;
            OpenPlayStorePage();
        }

        // True when Play started its flow (or is installing an update downloaded earlier)
        private async UniTask<bool> StartAsync(bool immediate, CancellationToken cancellation)
        {
            var done = new UniTaskCompletionSource<bool>();
            try
            {
                using var bridge = new AndroidJavaClass(Bridge);
                using var activity = CurrentActivity();
                bridge.CallStatic("start", activity, immediate, new Callback(done, log, immediate));
            }
            catch (Exception e)
            {
                log.Warning($"Google Play update unavailable: {e.Message}");
                return false;
            }
            var started = await done.Task.AttachExternalCancellation(cancellation);
            // Play answers on the Android UI thread
            await UniTask.SwitchToMainThread();
            return started;
        }

        private long? ReadVersionCode()
        {
            try
            {
                using var bridge = new AndroidJavaClass(Bridge);
                using var activity = CurrentActivity();
                var code = bridge.CallStatic<long>("versionCode", activity);
                return code >= 0 ? code : (long?)null;
            }
            catch (Exception e)
            {
                log.Warning($"Version code unreadable: {e.Message}");
                return null;
            }
        }

        private void OpenPlayStorePage()
        {
            try
            {
                using var review = new AndroidJavaClass("dev.cmpose.play.CmposeReview");
                using var activity = CurrentActivity();
                var id = Application.identifier;
                if (!review.CallStatic<bool>("openUrl", activity, StoreLinks.PlayApp(id), StoreLinks.PlayWeb(id))) log.Warning("No app can open the Play Store page");
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

        // dev.cmpose.play.CmposeCallback; called on the Android UI thread
        private sealed class Callback : AndroidJavaProxy
        {
            private readonly UniTaskCompletionSource<bool> done;
            private readonly ILog log;
            private readonly bool immediate;

            public Callback(UniTaskCompletionSource<bool> done, ILog log, bool immediate) : base("dev.cmpose.play.CmposeCallback")
            {
                this.done = done;
                this.log = log;
                this.immediate = immediate;
            }

            [Preserve]
            public void onResult(bool ok, string detail)
            {
                if (!ok) log.Warning($"Google Play {(immediate ? "immediate" : "flexible")} update not started: {detail}");
                done.TrySetResult(ok);
            }
        }
    }
}
#endif
