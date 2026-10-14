using System;
using System.Collections.Generic;
using Kturker.UnityBase.App.Game;
using Kturker.UnityBase.Features.Sample.Presentation;

namespace Kturker.UnityBase.Tests.EditMode
{
    // The sample's fakes; the internal 'sample' flag removes this file with the sample
    public sealed class FakeSampleView : ISampleView
    {
        public string Greeting;
        public int DoneShown;
        public int? Completed;
        public string Label { get; set; } = "Sample";
        public event Action BackClicked;
        public event Action CompleteClicked;
        public void ShowGreeting(string label) => Greeting = label;
        public void ShowDone() => DoneShown++;
        public void ShowCompleted(int count) => Completed = count;
        public void ClickBack() => BackClicked?.Invoke();
        public void ClickComplete() => CompleteClicked?.Invoke();
    }

    public sealed class FakeGuideView : IGuideView
    {
        public int Shown;
        public int Hidden;
        public event Action DismissClicked;
        public void Show() => Shown++;
        public void Hide() => Hidden++;
        public void ClickDismiss() => DismissClicked?.Invoke();
    }

    public sealed class FakeCompletionCounter : ICompletionCounter
    {
        public int Count;
        public int? Current => Count;
        public int? Record() => ++Count;
    }

    // cmpose:network
    public sealed class FakeGitHubUserCardView : Kturker.UnityBase.Features.Sample.Network.IGitHubUserCardView
    {
        public readonly List<string> Calls = new List<string>();
        public UnityEngine.Texture2D Avatar;
        public event Action RetryClicked;
        public void ShowLoading() => Calls.Add("loading");
        public void ShowUser(string displayName, string login) => Calls.Add($"user {displayName} {login}");

        public void ShowAvatar(UnityEngine.Texture2D avatar)
        {
            Avatar = avatar;
            Calls.Add("avatar");
        }

        public void ShowError(string key, long statusCode) => Calls.Add($"error {key} {statusCode}");
        public void Retry() => RetryClicked?.Invoke();
    }
    // /cmpose:network
    // cmpose:ads
    public sealed class FakeBonusView : Kturker.UnityBase.Features.Sample.Ads.IBonusView
    {
        public string Hint;
        public event Action BonusClicked;
        public void ShowHint(string key) => Hint = key;
        public void ClickBonus() => BonusClicked?.Invoke();
    }
    // /cmpose:ads
    // cmpose:leaderboards
    public sealed class FakeLeaderboardView : Kturker.UnityBase.Features.Sample.Leaderboards.ILeaderboardView
    {
        public string Hint;
        public int HintsHidden;
        public event Action LeaderboardClicked;
        public event Action AchievementsClicked;
        public void ShowHint(string key) => Hint = key;

        public void HideHint()
        {
            Hint = null;
            HintsHidden++;
        }

        public void ClickLeaderboard() => LeaderboardClicked?.Invoke();
        public void ClickAchievements() => AchievementsClicked?.Invoke();
    }
    // /cmpose:leaderboards
}
