using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Navigation
{
    // Runs the registered boot steps in order, then opens the menu. A failed step or a menu that fails to open
    // shows the error view; Retry resumes from that point, so finished steps never run twice. A cancelled run
    // (the scope is going away) ends quietly.
    public sealed class BootEntry : IAsyncStartable
    {
        public const string StepFailedKey = "boot.error.step";
        public const string MenuFailedKey = "boot.error.menu";

        private readonly IReadOnlyList<IBootStep> steps;
        private readonly ISceneLoader loader;
        private readonly IBootErrorView errorView;
        private readonly ILog log;
        private int next;
        private bool running;

        public BootEntry(IReadOnlyList<IBootStep> steps, ISceneLoader loader, IBootErrorView errorView, ILog log)
        {
            this.steps = steps;
            this.loader = loader;
            this.errorView = errorView;
            this.log = log;
        }

        public UniTask StartAsync(CancellationToken cancellation) => RunAsync(cancellation);

        public async UniTask RunAsync(CancellationToken cancellation)
        {
            if (running)
            {
                log.Warning("Boot retry ignored while a run is active");
                return;
            }
            running = true;
            try
            {
                while (next < steps.Count)
                {
                    var step = steps[next];
                    try
                    {
                        await step.RunAsync(cancellation);
                    }
                    catch (OperationCanceledException)
                    {
                        log.Info($"Boot cancelled during: {step.Name}");
                        return;
                    }
                    catch (Exception e)
                    {
                        log.Error($"Boot step failed: {step.Name}", e);
                        ShowError(StepFailedKey, step.Name, cancellation);
                        return;
                    }
                    next++;
                }
                try
                {
                    await loader.LoadAsync(SceneId.Menu, cancellation);
                }
                catch (OperationCanceledException)
                {
                    log.Info("Boot cancelled while opening the menu");
                }
                catch (Exception e)
                {
                    log.Error("Menu failed to open", e);
                    ShowError(MenuFailedKey, null, cancellation);
                }
            }
            finally
            {
                running = false;
            }
        }

        private void ShowError(string messageKey, string detail, CancellationToken cancellation) =>
            errorView.Show(messageKey, detail, () => RunAsync(cancellation).Forget());
    }
}
