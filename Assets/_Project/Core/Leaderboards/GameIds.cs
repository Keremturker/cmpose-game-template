using System.Collections.Generic;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // The IDs of one platform: the high-score leaderboard and the achievements
    public sealed class GameIds
    {
        public GameIds(string leaderboard, IReadOnlyList<AchievementId> achievements)
        {
            Leaderboard = leaderboard;
            Achievements = achievements;
        }

        public string Leaderboard { get; }
        public IReadOnlyList<AchievementId> Achievements { get; }

        public bool TryFind(string key, out AchievementId id)
        {
            foreach (var candidate in Achievements)
            {
                if (candidate.Key != key) continue;
                id = candidate;
                return true;
            }
            id = default;
            return false;
        }
    }
}
