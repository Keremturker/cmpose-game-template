using Kturker.UnityBase.App.Settings;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Sample.Installer;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Game
{
    // Game scene: pause, guide and settings panels, then the feature modules; the generator adds one line per
    // cloned module inside the markers
    public sealed class GameInstaller : IInstaller
    {
        private readonly bool registerViews;

        public GameInstaller(bool registerViews) => this.registerViews = registerViews;

        public void Install(IContainerBuilder builder)
        {
            if (registerViews)
            {
                builder.RegisterComponentInHierarchy<SettingsView>().As<ISettingsView>();
                builder.RegisterComponentInHierarchy<PauseView>().As<IPauseView>();
                builder.RegisterComponentInHierarchy<GuideView>().As<IGuideView>();
            }
            builder.RegisterEntryPoint<SceneUiBinding>();
            builder.RegisterEntryPoint<SettingsPresenter>();
            builder.RegisterEntryPoint<PausePresenter>();
            builder.RegisterEntryPoint<GuidePresenter>();
            builder.RegisterEntryPoint<GameRenderMarker>();
            // cmpose:features:start
            new SampleInstaller(registerViews).Install(builder);
            // cmpose:features:end
        }
    }
}
