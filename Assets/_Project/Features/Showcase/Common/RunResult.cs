namespace Kturker.UnityBase.Features.Showcase.Common
{
    // One run's outcome: the score (distance plus the stars' bonus), the distance and the stars collected
    public readonly struct RunResult
    {
        public RunResult(int score, int distance, int stars)
        {
            Score = score;
            Distance = distance;
            Stars = stars;
        }

        public int Score { get; }
        public int Distance { get; }
        public int Stars { get; }
    }
}
