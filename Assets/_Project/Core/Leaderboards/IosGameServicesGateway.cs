#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // Game Center through Plugins/iOS/CmposeGameCenter.mm. GameKit answers on its own queues, so every call polls the
    // bridge and stays on Unity's main thread. A bridge error is logged once and turns game services off for the run.
    public sealed class IosGameServicesGateway : IGameServicesGateway
    {
        private const int PollMilliseconds = 100;
        // GameKit answers the silent sign-in within a few seconds; an operation within twenty
        private const int StartPolls = 100;
        private const int SettlePolls = 20;
        private const int OperationPolls = 200;

        [DllImport("__Internal")]
        private static extern void CmposeGcStart();

        [DllImport("__Internal")]
        private static extern int CmposeGcAuthState();

        [DllImport("__Internal")]
        private static extern int CmposeGcNeedsLogin();

        [DllImport("__Internal")]
        private static extern void CmposeGcShowLogin();

        [DllImport("__Internal")]
        private static extern int CmposeGcLoginVisible();

        [DllImport("__Internal")]
        private static extern int CmposeGcSubmitScore(string leaderboardId, long score);

        [DllImport("__Internal")]
        private static extern int CmposeGcUnlock(string achievementId);

        [DllImport("__Internal")]
        private static extern int CmposeGcIncrement(string achievementId, int steps, int totalSteps);

        [DllImport("__Internal")]
        private static extern int CmposeGcShowLeaderboard(string leaderboardId);

        [DllImport("__Internal")]
        private static extern int CmposeGcShowAchievements();

        [DllImport("__Internal")]
        private static extern int CmposeGcResult(int token);

        private readonly GameIds ids;
        private readonly ILog log;
        private bool failed;

        public IosGameServicesGateway(GameIds ids, ILog log)
        {
            this.ids = ids;
            this.log = log;
        }

        public UniTask<bool> StartAsync() => GuardAsync("start", async () =>
        {
            CmposeGcStart();
            for (var i = 0; i < StartPolls && CmposeGcAuthState() == 0; i++) await Wait();
            return CmposeGcAuthState() == 1;
        });

        public UniTask<bool> SignInAsync() => GuardAsync("signIn", async () =>
        {
            if (CmposeGcAuthState() == 1) return true;
            // Nothing to show: GameKit has not answered yet, the player is restricted or Game Center is off
            if (CmposeGcNeedsLogin() == 0) return false;
            CmposeGcShowLogin();
            while (CmposeGcLoginVisible() == 1) await Wait();
            // GameKit calls its handler again once the player is done
            for (var i = 0; i < SettlePolls && CmposeGcAuthState() != 1; i++) await Wait();
            return CmposeGcAuthState() == 1;
        });

        public UniTask<bool> SubmitScoreAsync(long score) =>
            GuardAsync("submitScore", () => Operation(CmposeGcSubmitScore(ids.Leaderboard, score)));

        public UniTask<bool> UnlockAsync(string key) => GuardAsync("unlock", () =>
        {
            if (!ids.TryFind(key, out var achievement)) return UnknownKey(key);
            return Operation(CmposeGcUnlock(achievement.Id));
        });

        public UniTask<bool> IncrementAsync(string key, int steps) => GuardAsync("increment", () =>
        {
            if (!ids.TryFind(key, out var achievement)) return UnknownKey(key);
            return Operation(CmposeGcIncrement(achievement.Id, steps, Math.Max(1, achievement.Steps)));
        });

        public UniTask<bool> ShowLeaderboardAsync() =>
            GuardAsync("showLeaderboard", () => Operation(CmposeGcShowLeaderboard(ids.Leaderboard)));

        public UniTask<bool> ShowAchievementsAsync() =>
            GuardAsync("showAchievements", () => Operation(CmposeGcShowAchievements()));

        private UniTask<bool> UnknownKey(string key)
        {
            log.Warning($"Game services have no achievement for the key {key}");
            return UniTask.FromResult(false);
        }

        private static UniTask Wait() => UniTask.Delay(PollMilliseconds, ignoreTimeScale: true);

        private static async UniTask<bool> Operation(int token)
        {
            for (var i = 0; i < OperationPolls; i++)
            {
                var result = CmposeGcResult(token);
                if (result != 0) return result == 1;
                await Wait();
            }
            return false;
        }

        private async UniTask<bool> GuardAsync(string name, Func<UniTask<bool>> action)
        {
            if (failed) return false;
            try
            {
                return await action();
            }
            catch (Exception e)
            {
                failed = true;
                log.Warning($"Game services turned off after a bridge error ({name}): {e.Message}");
                return false;
            }
        }
    }
}
#endif
