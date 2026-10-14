using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Leaderboards.Tests
{
    // Completes synchronously; Calls records what reached the platform, in order
    public sealed class FakeGameServicesGateway : IGameServicesGateway
    {
        public readonly List<string> Calls = new List<string>();
        public bool SilentSignIn;
        public bool InteractiveSignIn = true;
        public bool Succeeds = true;
        public bool ScreenOpens = true;
        // When set, the next SubmitScoreAsync / screen waits for it (a one-shot)
        public UniTaskCompletionSource<bool> PendingSubmit;
        public UniTaskCompletionSource<bool> PendingScreen;

        public UniTask<bool> StartAsync()
        {
            Calls.Add("start");
            return UniTask.FromResult(SilentSignIn);
        }

        public UniTask<bool> SignInAsync()
        {
            Calls.Add("signIn");
            return UniTask.FromResult(InteractiveSignIn);
        }

        public UniTask<bool> SubmitScoreAsync(long score)
        {
            Calls.Add($"submit {score}");
            return PendingSubmit != null ? Take(ref PendingSubmit) : UniTask.FromResult(Succeeds);
        }

        public UniTask<bool> UnlockAsync(string key)
        {
            Calls.Add($"unlock {key}");
            return UniTask.FromResult(Succeeds);
        }

        public UniTask<bool> IncrementAsync(string key, int steps)
        {
            Calls.Add($"increment {key} {steps}");
            return UniTask.FromResult(Succeeds);
        }

        public UniTask<bool> ShowLeaderboardAsync()
        {
            Calls.Add("showLeaderboard");
            return PendingScreen != null ? Take(ref PendingScreen) : UniTask.FromResult(ScreenOpens);
        }

        public UniTask<bool> ShowAchievementsAsync()
        {
            Calls.Add("showAchievements");
            return PendingScreen != null ? Take(ref PendingScreen) : UniTask.FromResult(ScreenOpens);
        }

        private static UniTask<bool> Take(ref UniTaskCompletionSource<bool> pending)
        {
            var task = pending.Task;
            pending = null;
            return task;
        }
    }

    // For the settings row and the sample: records the calls and answers Result
    public sealed class FakeGameServices : IGameServices
    {
        public readonly List<string> Calls = new List<string>();
        public bool Available = true;
        public bool SignedIn;
        public GameServicesResult Result = GameServicesResult.Ok;
        // When set, the next SubmitScoreAsync waits for it (a one-shot)
        public UniTaskCompletionSource<GameServicesResult> PendingScore;
        public int SignIns;

        public bool IsAvailable => Available;
        public bool IsSignedIn => SignedIn;
        public event Action Changed;
        public void RaiseChanged() => Changed?.Invoke();

        public UniTask<GameServicesResult> SignInAsync(CancellationToken token)
        {
            SignIns++;
            return Done();
        }

        public UniTask<GameServicesResult> SubmitScoreAsync(long score, CancellationToken token)
        {
            Calls.Add($"score {score}");
            if (PendingScore == null) return Done();
            var task = PendingScore.Task;
            PendingScore = null;
            return task;
        }

        public UniTask<GameServicesResult> ShowLeaderboardAsync(CancellationToken token)
        {
            Calls.Add("showLeaderboard");
            return Done();
        }

        public UniTask<GameServicesResult> UnlockAchievementAsync(string key, CancellationToken token)
        {
            Calls.Add($"unlock {key}");
            return Done();
        }

        public UniTask<GameServicesResult> IncrementAchievementAsync(string key, int steps, CancellationToken token)
        {
            Calls.Add($"increment {key} {steps}");
            return Done();
        }

        public UniTask<GameServicesResult> ShowAchievementsAsync(CancellationToken token)
        {
            Calls.Add("showAchievements");
            return Done();
        }

        private UniTask<GameServicesResult> Done() => UniTask.FromResult(Result);
    }
}
