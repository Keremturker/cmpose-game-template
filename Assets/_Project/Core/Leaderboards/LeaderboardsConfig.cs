using System;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // Leaderboard and achievement IDs per platform. The generator writes the six ID lines (one const string each, the
    // ID in double quotes); the Android app id is in Plugins/Android/CmposeGames.androidlib/src/main/res/values/
    // games-ids.xml. The template ships placeholders: while any ID is a placeholder (or in the editor) the service
    // stays off and the game carries on without scores. Set the real IDs before you ship (see README).
    public static class LeaderboardsConfig
    {
        public const string LeaderboardAndroid = "REPLACE_ME_high_score";
        public const string FirstCompletionAndroid = "REPLACE_ME_first_completion";
        public const string TenCompletionsAndroid = "REPLACE_ME_ten_completions";
        public const string LeaderboardIos = "cmpose.replace_me.high_score";
        public const string FirstCompletionIos = "cmpose.replace_me.first_completion";
        public const string TenCompletionsIos = "cmpose.replace_me.ten_completions";

        // The total steps of the "10 completions" achievement; Play Games keeps its own total from the console
        public const int TenCompletionsSteps = 10;

        public static GameIds Ids(bool ios) => ios
            ? new GameIds(LeaderboardIos, Achievements(FirstCompletionIos, TenCompletionsIos))
            : new GameIds(LeaderboardAndroid, Achievements(FirstCompletionAndroid, TenCompletionsAndroid));

        public static bool IsConfigured(bool ios) => IsConfigured(Ids(ios));

        public static bool IsConfigured(GameIds ids)
        {
            if (IsPlaceholder(ids.Leaderboard)) return false;
            foreach (var achievement in ids.Achievements)
            {
                if (IsPlaceholder(achievement.Id)) return false;
            }
            return true;
        }

        public static bool IsPlaceholder(string id) =>
            string.IsNullOrWhiteSpace(id) || id.IndexOf("replace_me", StringComparison.OrdinalIgnoreCase) >= 0;

        private static AchievementId[] Achievements(string first, string ten) => new[]
        {
            new AchievementId(AchievementKeys.FirstCompletion, first, 0),
            new AchievementId(AchievementKeys.TenCompletions, ten, TenCompletionsSteps)
        };
    }
}
