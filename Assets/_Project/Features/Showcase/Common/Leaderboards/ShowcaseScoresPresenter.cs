using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Leaderboards;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Features.Showcase.Common.Leaderboards
{
    // The leaderboards flag in the showcase's menu: two buttons open the board and the achievements (signing in first
    // when needed); "Not set up" while the service is off. Leaving the scene cancels.
    public sealed class ShowcaseScoresPresenter : Presenter<IShowcaseScoresView>
    {
        private readonly IGameServices services;
        private readonly IUiFeedback feedback;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        public ShowcaseScoresPresenter(IShowcaseScoresView view, IGameServices services, IUiFeedback feedback) : base(view)
        {
            this.services = services;
            this.feedback = feedback;
        }

        public override void Start()
        {
            View.LeaderboardClicked += OnLeaderboard;
            View.AchievementsClicked += OnAchievements;
        }

        public override void Dispose()
        {
            View.LeaderboardClicked -= OnLeaderboard;
            View.AchievementsClicked -= OnAchievements;
            cancellation.Cancel();
            cancellation.Dispose();
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
