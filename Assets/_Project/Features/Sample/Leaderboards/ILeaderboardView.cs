using System;

namespace Kturker.UnityBase.Features.Sample.Leaderboards
{
    public interface ILeaderboardView
    {
        event Action LeaderboardClicked;
        event Action AchievementsClicked;
        void ShowHint(string key);
        void HideHint();
    }
}
