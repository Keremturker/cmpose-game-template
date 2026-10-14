using System;

namespace Kturker.UnityBase.Features.Showcase.Common.Leaderboards
{
    public interface IShowcaseScoresView
    {
        event Action LeaderboardClicked;
        event Action AchievementsClicked;
        void ShowHint(string key);
        void HideHint();
    }
}
