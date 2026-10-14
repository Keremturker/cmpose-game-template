using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // The only place that touches the platform. Answers arrive on Unity's main thread; nothing throws (a failure is
    // a warning in the log and false). Achievement keys are AchievementKeys; the gateway maps them to platform IDs.
    public interface IGameServicesGateway
    {
        // Silent sign-in at start: true when the player is signed in, never shows a screen
        UniTask<bool> StartAsync();

        // The player asked: may show the platform's sign-in screen; true when signed in afterwards
        UniTask<bool> SignInAsync();

        UniTask<bool> SubmitScoreAsync(long score);

        UniTask<bool> UnlockAsync(string key);

        UniTask<bool> IncrementAsync(string key, int steps);

        // True when the screen was opened (not when the player closes it)
        UniTask<bool> ShowLeaderboardAsync();

        UniTask<bool> ShowAchievementsAsync();
    }
}
