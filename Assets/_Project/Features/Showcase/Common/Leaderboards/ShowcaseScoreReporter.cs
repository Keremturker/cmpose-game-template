using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Leaderboards;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Showcase.Common.Leaderboards
{
    // The leaderboards flag in a showcase game scene: every finished run reports its score, unlocks "first completion"
    // (the first finished run) and moves "ten completions" a step. The three calls start together: GameServices keeps
    // each one before it waits for the network, so leaving the scene right after a run loses none of them.
    public sealed class ShowcaseScoreReporter : IStartable, IDisposable
    {
        private readonly IShowcaseRun run;
        private readonly IGameServices services;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        public ShowcaseScoreReporter(IShowcaseRun run, IGameServices services)
        {
            this.run = run;
            this.services = services;
        }

        public void Start() => run.Finished += OnFinished;

        public void Dispose()
        {
            run.Finished -= OnFinished;
            cancellation.Cancel();
            cancellation.Dispose();
        }

        private void OnFinished(RunResult result) => ReportAsync(result.Score).Forget();

        private async UniTaskVoid ReportAsync(int score)
        {
            var token = cancellation.Token;
            try
            {
                await UniTask.WhenAll(
                    services.SubmitScoreAsync(score, token),
                    services.UnlockAchievementAsync(AchievementKeys.FirstCompletion, token),
                    services.IncrementAchievementAsync(AchievementKeys.TenCompletions, 1, token));
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
