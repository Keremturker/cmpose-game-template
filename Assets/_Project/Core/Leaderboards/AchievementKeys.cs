namespace Kturker.UnityBase.Core.Leaderboards
{
    // Logical achievement keys the game uses; LeaderboardsConfig maps them to each platform's IDs. A key never
    // contains a comma (GameServices keeps sets of keys as comma-separated text).
    public static class AchievementKeys
    {
        public const string FirstCompletion = "firstCompletion";
        public const string TenCompletions = "tenCompletions";
    }
}
