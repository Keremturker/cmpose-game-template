using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Leaderboards;
using Kturker.UnityBase.Core.Preferences;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Sample.Presentation;

namespace Kturker.UnityBase.Features.Sample.Leaderboards
{
    // The leaderboards flag's example in the Game scene: every Complete reports its running count as the score, the
    // first one unlocks "First completion" and each one moves "10 completions" a step; two buttons open the board and
    // the achievements (signing in first when needed). The count is the sample's own, so it works with the save system
    // off. Leaving the scene cancels; a late result changes nothing.
    public sealed class SampleLeaderboardsPresenter : Presenter<ILeaderboardView>
    {
        public const string CompletionsKey = "sample.leaderboards.completions";

        private readonly ISampleView sample;
        private readonly IGameServices services;
        private readonly IPreferences preferences;
        private readonly IUiFeedback feedback;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        public SampleLeaderboardsPresenter(ILeaderboardView view, ISampleView sample, IGameServices services,
            IPreferences preferences, IUiFeedback feedback) : base(view)
        {
            this.sample = sample;
            this.services = services;
            this.preferences = preferences;
            this.feedback = feedback;
        }

        public override void Start()
        {
            sample.CompleteClicked += OnComplete;
            View.LeaderboardClicked += OnLeaderboard;
            View.AchievementsClicked += OnAchievements;
        }

        public override void Dispose()
        {
            sample.CompleteClicked -= OnComplete;
            View.LeaderboardClicked -= OnLeaderboard;
            View.AchievementsClicked -= OnAchievements;
            cancellation.Cancel();
            cancellation.Dispose();
        }

        private void OnComplete()
        {
            var completions = preferences.GetInt(CompletionsKey, 0) + 1;
            preferences.SetInt(CompletionsKey, completions);
            preferences.Save();
            ReportAsync(completions).Forget();
        }

        private async UniTaskVoid ReportAsync(int completions)
        {
            var token = cancellation.Token;
            try
            {
                await services.SubmitScoreAsync(completions, token);
                if (token.IsCancellationRequested) return;
                if (completions == 1) await services.UnlockAchievementAsync(AchievementKeys.FirstCompletion, token);
                if (token.IsCancellationRequested) return;
                await services.IncrementAchievementAsync(AchievementKeys.TenCompletions, 1, token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnLeaderboard() => ShowAsync(services.ShowLeaderboardAsync).Forget();

        private void OnAchievements() => ShowAsync(services.ShowAchievementsAsync).Forget();

        private async UniTaskVoid ShowAsync(Func<CancellationToken, UniTask<GameServicesResult>> show)
        {
            feedback.Click();
            if (!services.IsAvailable)
            {
                View.ShowHint(LeaderboardsKeys.NotSetUp);
                return;
            }
            View.HideHint();
            try
            {
                await show(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
