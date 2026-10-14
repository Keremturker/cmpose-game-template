using Cysharp.Threading.Tasks;
using Kturker.UnityBase.App.Settings;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Lifecycle;
using Kturker.UnityBase.Core.Navigation;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.App.Game
{
    // The pause button pauses with PauseReason.User and opens the panel; losing focus pauses without it.
    // Leaving the scene while paused (the Menu button or a deep link) always ends the user pause, so the next
    // scene never starts at timeScale 0.
    public sealed class PausePresenter : Presenter<IPauseView>
    {
        private readonly PauseService pause;
        private readonly ISettingsPanel settings;
        private readonly ISceneLoader loader;
        private readonly IUiFeedback feedback;

        public PausePresenter(IPauseView view, PauseService pause, ISettingsPanel settings, ISceneLoader loader, IUiFeedback feedback)
            : base(view)
        {
            this.pause = pause;
            this.settings = settings;
            this.loader = loader;
            this.feedback = feedback;
        }

        public override void Start()
        {
            View.PauseClicked += OnPause;
            View.ResumeClicked += OnResume;
            View.SettingsClicked += OnSettings;
            View.MenuClicked += OnMenu;
        }

        public override void Dispose()
        {
            View.PauseClicked -= OnPause;
            View.ResumeClicked -= OnResume;
            View.SettingsClicked -= OnSettings;
            View.MenuClicked -= OnMenu;
            pause.Resume(PauseReason.User);
        }

        private void OnPause()
        {
            if (pause.Has(PauseReason.User)) return;
            feedback.Click();
            pause.Pause(PauseReason.User);
            View.ShowPanel();
        }

        private void OnResume()
        {
            if (!pause.Has(PauseReason.User)) return;
            feedback.Click();
            View.HidePanel();
            pause.Resume(PauseReason.User);
        }

        private void OnSettings()
        {
            feedback.Click();
            settings.Open();
        }

        private void OnMenu()
        {
            feedback.Click();
            View.HidePanel();
            pause.Resume(PauseReason.User);
            loader.LoadAsync(SceneId.Menu).Forget();
        }
    }
}
