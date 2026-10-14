namespace Kturker.UnityBase.Core.Leaderboards
{
    public readonly struct AchievementId
    {
        public AchievementId(string key, string id, int steps)
        {
            Key = key;
            Id = id;
            Steps = steps;
        }

        public string Key { get; }

        // The platform's achievement ID
        public string Id { get; }

        // 0: unlocked in one go; otherwise the total steps of an incremental achievement
        public int Steps { get; }
    }
}
