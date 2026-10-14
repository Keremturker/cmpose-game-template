using System;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    // Shows the game-over panel at a crash and hides it when the run continues. Retry and Menu end the run for good
    // (Finish, which stores the best score); Retry then starts a new run, Menu goes back to the menu.
    public sealed class GameOverPresenter : Presenter<IGameOverView>
    {
        private readonly IShowcaseRun run;
        private readonly BestScore best;
        private readonly ISceneLoader loader;
        private readonly IUiFeedback feedback;

        public GameOverPresenter(IGameOverView view, IShowcaseRun run, BestScore best, ISceneLoader loader, IUiFeedback feedback)
            : base(view)
        {
            this.run = run;
            this.best = best;
            this.loader = loader;
            this.feedback = feedback;
        }

        public override void Start()
        {
            run.Crashed += OnCrashed;
            run.Continued += OnContinued;
            run.Finished += OnFinished;
            View.RetryClicked += OnRetry;
            View.MenuClicked += OnMenu;
        }

        public override void Dispose()
        {
            run.Crashed -= OnCrashed;
            run.Continued -= OnContinued;
            run.Finished -= OnFinished;
            View.RetryClicked -= OnRetry;
            View.MenuClicked -= OnMenu;
        }

        private void OnCrashed(RunResult result)
        {
            var stored = best.Value;
            View.Show(result.Score, Math.Max(stored, result.Score), result.Score > stored);
        }

        private void OnContinued() => View.Hide();

        private void OnFinished(RunResult result) => best.Submit(result.Score);

        private void OnRetry()
        {
            if (!run.IsCrashed) return;
            feedback.Click();
            View.Hide();
            run.Finish();
            run.Restart();
        }

        private void OnMenu()
        {
            if (!run.IsCrashed) return;
            feedback.Click();
            run.Finish();
            loader.LoadAsync(SceneId.Menu).Forget();
        }
    }
}
