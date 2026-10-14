namespace Kturker.UnityBase.Core.Update
{
    public static class UpdateSchedule
    {
        public const long OneDayMillis = 24L * 60 * 60 * 1000;

        // The first launch checks, then at most once a day. While an update is required every launch checks, so
        // reopening the game does not get past the panel. A clock set back (a last check "in the future") checks again.
        public static bool IsDue(long? lastCheckMillis, long nowMillis, bool lastWasRequired)
        {
            if (lastWasRequired || !lastCheckMillis.HasValue) return true;
            var elapsed = nowMillis - lastCheckMillis.Value;
            return elapsed >= OneDayMillis || elapsed < 0;
        }
    }
}
