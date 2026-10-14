using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // The editor and placeholder IDs: there is no platform, so nothing succeeds (GameServices never calls it then)
    public sealed class NoGameServicesGateway : IGameServicesGateway
    {
        public UniTask<bool> StartAsync() => UniTask.FromResult(false);

        public UniTask<bool> SignInAsync() => UniTask.FromResult(false);

        public UniTask<bool> SubmitScoreAsync(long score) => UniTask.FromResult(false);

        public UniTask<bool> UnlockAsync(string key) => UniTask.FromResult(false);

        public UniTask<bool> IncrementAsync(string key, int steps) => UniTask.FromResult(false);

        public UniTask<bool> ShowLeaderboardAsync() => UniTask.FromResult(false);

        public UniTask<bool> ShowAchievementsAsync() => UniTask.FromResult(false);
    }
}
