using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Update
{
    // Runs the update check in the background when the menu first opens (boot is not delayed) and keeps the decision
    // for the menu's update panel. While an update is required, any other scene sends the player back to the menu, so
    // neither Play nor a deep link gets past it. On Android, with nothing required, Google Play's flexible update
    // starts when Play offers one.
    public sealed class UpdateFlow : IStartable, IDisposable
    {
        private readonly ISceneLoader loader;
        private readonly UpdateChecker checker;
        private readonly IPlatformUpdater platform;
        private readonly ILog log;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private SceneId scene = SceneId.Boot;
        private bool started;
        private bool disposed;

        public UpdateFlow(ISceneLoader loader, UpdateChecker checker, IPlatformUpdater platform, ILog log)
        {
            this.loader = loader;
            this.checker = checker;
            this.platform = platform;
            this.log = log;
        }

        // Null until a check has decided (and when none was due)
        public UpdateDecision Current { get; private set; }

        public event Action<UpdateDecision> Decided;

        private bool IsRequired => Current != null && Current.Kind == UpdateKind.Required;

        public void Start() => loader.Loaded += OnLoaded;

        // Later on the "Update available" panel; a required update cannot be dismissed
        public void Dismiss()
        {
            if (Current != null && Current.Kind == UpdateKind.Available) Current = UpdateDecision.None;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            loader.Loaded -= OnLoaded;
            cancellation.Cancel();
            cancellation.Dispose();
        }

        private void OnLoaded(SceneId id)
        {
            scene = id;
            if (id == SceneId.Menu)
            {
                if (started) return;
                started = true;
                RunAsync().Forget();
                return;
            }
            if (IsRequired) ReturnToMenu();
        }

        private void ReturnToMenu()
        {
            log.Warning("Update required: back to the menu");
            loader.LoadAsync(SceneId.Menu).Forget();
        }

        private async UniTaskVoid RunAsync()
        {
            UpdateDecision decision;
            try
            {
                decision = await checker.CheckAsync(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                log.Warning($"Update check failed: {e.Message}");
                return;
            }
            if (disposed || decision == null) return;
            Current = decision;
            Decided?.Invoke(decision);
            if (decision.Kind == UpdateKind.Required)
            {
                // A scene load still running finishes first; its Loaded event then sends the player back
                if (scene != SceneId.Menu) ReturnToMenu();
                return;
            }
            if (decision.Kind == UpdateKind.None && platform.Platform == StorePlatform.Android) await StartFlexibleAsync();
        }

        private async UniTask StartFlexibleAsync()
        {
            try
            {
                if (await platform.StartFlexibleUpdateAsync(cancellation.Token)) log.Info("Google Play flexible update started");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                log.Warning($"Google Play update unavailable: {e.Message}");
            }
        }
    }
}
