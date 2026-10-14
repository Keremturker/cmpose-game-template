using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // The game's one entry to the platform's game services (Google Play Games, Game Center): a high-score board and
    // achievements. Nothing here throws or holds up the game; a value that cannot be sent yet is kept and sent at the
    // next sign-in. A cancelled token throws OperationCanceledException before anything starts.
    public interface IGameServices
    {
        // False in the editor and while the IDs in LeaderboardsConfig are placeholders: every call is Unavailable
        bool IsAvailable { get; }

        bool IsSignedIn { get; }

        // After the sign-in state changed
        event Action Changed;

        // The player asked: the platform may show its sign-in screen
        UniTask<GameServicesResult> SignInAsync(CancellationToken token);

        UniTask<GameServicesResult> SubmitScoreAsync(long score, CancellationToken token);

        UniTask<GameServicesResult> ShowLeaderboardAsync(CancellationToken token);

        UniTask<GameServicesResult> UnlockAchievementAsync(string key, CancellationToken token);

        UniTask<GameServicesResult> IncrementAchievementAsync(string key, int steps, CancellationToken token);

        UniTask<GameServicesResult> ShowAchievementsAsync(CancellationToken token);
    }
}
