using Kturker.UnityBase.App.Game;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Lifecycle;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class PausePresenterTests
    {
        private PauseService pause;
        private FakePauseView view;
        private FakeSettingsPanel settings;
        private FakeSceneLoader loader;
        private FakeUiFeedback feedback;
        private PausePresenter presenter;

        [SetUp]
        public void Create()
        {
            pause = new PauseService();
            view = new FakePauseView();
            settings = new FakeSettingsPanel();
            loader = new FakeSceneLoader();
            feedback = new FakeUiFeedback();
            presenter = new PausePresenter(view, pause, settings, loader, feedback);
            presenter.Start();
        }

        [Test]
        public void Pause_button_pauses_once_and_shows_the_panel_and_resume_undoes_it()
        {
            view.ClickPause();
            view.ClickPause();

            Assert.That(pause.Has(PauseReason.User), Is.True);
            Assert.That(view.PanelShown, Is.EqualTo(1));

            view.ClickSettings();
            Assert.That(settings.Opened, Is.EqualTo(1));

            view.ClickResume();
            view.ClickResume();

            Assert.That(pause.IsPaused, Is.False);
            Assert.That(view.PanelHidden, Is.EqualTo(1));
            Assert.That(feedback.Clicks, Is.EqualTo(3));
        }

        [Test]
        public void Menu_resumes_before_it_loads_the_menu()
        {
            var pausedWhenLoading = true;
            loader.OnLoad = _ => pausedWhenLoading = pause.IsPaused;

            view.ClickPause();
            view.ClickMenu();

            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
            Assert.That(pausedWhenLoading, Is.False);
            Assert.That(view.PanelHidden, Is.EqualTo(1));
        }

        [Test]
        public void Losing_focus_pauses_without_opening_the_panel()
        {
            pause.Pause(PauseReason.Focus);

            Assert.That(pause.IsPaused, Is.True);
            Assert.That(view.PanelShown, Is.EqualTo(0));
        }

        [Test]
        public void Leaving_the_scene_while_paused_ends_the_user_pause_but_not_focus()
        {
            view.ClickPause();
            pause.Pause(PauseReason.Focus);

            presenter.Dispose();

            Assert.That(pause.Has(PauseReason.User), Is.False);
            Assert.That(pause.Has(PauseReason.Focus), Is.True);
            view.ClickPause();
            Assert.That(pause.Has(PauseReason.User), Is.False);
        }
    }
}
