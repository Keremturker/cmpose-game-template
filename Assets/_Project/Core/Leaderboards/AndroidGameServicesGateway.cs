#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // Google Play Games Services v2 through Plugins/Android/CmposeGames.androidlib (dev.cmpose.games.CmposeGames).
    // The bridge answers on the Android UI thread; every answer is moved to Unity's main thread. A bridge error (class
    // missing, JNI failure) is logged once and turns game services off for the run: false from then on.
    public sealed class AndroidGameServicesGateway : IGameServicesGateway
    {
        private const string Bridge = "dev.cmpose.games.CmposeGames";

        private readonly GameIds ids;
        private readonly ILog log;
        private UniTask<bool> initialization;
        private bool initializationStarted;
        private bool failed;

        public AndroidGameServicesGateway(GameIds ids, ILog log)
        {
            this.ids = ids;
            this.log = log;
        }

        public UniTask<bool> StartAsync() => InvokeAsync("isAuthenticated");

        public UniTask<bool> SignInAsync() => InvokeAsync("signIn");

        public UniTask<bool> SubmitScoreAsync(long score) => InvokeAsync("submitScore", ids.Leaderboard, score);

        public UniTask<bool> UnlockAsync(string key)
        {
            if (!ids.TryFind(key, out var achievement)) return UnknownKey(key);
            return InvokeAsync("unlock", achievement.Id);
        }

        public UniTask<bool> IncrementAsync(string key, int steps)
        {
            if (!ids.TryFind(key, out var achievement)) return UnknownKey(key);
            return InvokeAsync("increment", achievement.Id, steps);
        }

        public UniTask<bool> ShowLeaderboardAsync() => InvokeAsync("showLeaderboard", ids.Leaderboard);

        public UniTask<bool> ShowAchievementsAsync() => InvokeAsync("showAchievements");

        private UniTask<bool> UnknownKey(string key)
        {
            log.Warning($"Game services have no achievement for the key {key}");
            return UniTask.FromResult(false);
        }

        private async UniTask<bool> InvokeAsync(string method, params object[] args)
        {
            if (!await EnsureInitializedAsync()) return false;
            return await CallBridgeAsync(method, args);
        }

        // PlayGamesSdk.initialize runs once, before the first request, and only because the IDs are real
        private UniTask<bool> EnsureInitializedAsync()
        {
            if (!initializationStarted)
            {
                initializationStarted = true;
                initialization = CallBridgeAsync("initialize").Preserve();
            }
            return initialization;
        }

        private async UniTask<bool> CallBridgeAsync(string method, params object[] args)
        {
            if (failed) return false;
            var source = new UniTaskCompletionSource<(bool Ok, string Detail)>();
            try
            {
                using var bridge = new AndroidJavaClass(Bridge);
                using var activity = CurrentActivity();
                var all = new object[args.Length + 2];
                all[0] = activity;
                Array.Copy(args, 0, all, 1, args.Length);
                all[all.Length - 1] = new Callback(source);
                bridge.CallStatic(method, all);
            }
            catch (Exception e)
            {
                failed = true;
                log.Warning($"Game services turned off after a bridge error ({method}): {e.Message}");
                return false;
            }
            var (ok, detail) = await source.Task;
            await UniTask.SwitchToMainThread();
            if (!ok && !string.IsNullOrEmpty(detail)) log.Warning($"Game services {method} did not succeed: {detail}");
            return ok;
        }

        private static AndroidJavaObject CurrentActivity()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return player.GetStatic<AndroidJavaObject>("currentActivity");
        }

        // dev.cmpose.games.CmposeGamesCallback; called on the Android UI thread, so it only completes the source
        private sealed class Callback : AndroidJavaProxy
        {
            private readonly UniTaskCompletionSource<(bool Ok, string Detail)> source;

            public Callback(UniTaskCompletionSource<(bool Ok, string Detail)> source) : base("dev.cmpose.games.CmposeGamesCallback") =>
                this.source = source;

            [Preserve]
            public void onResult(bool ok, string detail) => source.TrySetResult((ok, detail));
        }
    }
}
#endif
