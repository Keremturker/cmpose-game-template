using Kturker.UnityBase.Core.Navigation;
using Kturker.UnityBase.Core.Presentation;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Boot
{
    public sealed class BootInstaller : IInstaller
    {
        private readonly bool registerViews;

        public BootInstaller(bool registerViews) => this.registerViews = registerViews;

        public void Install(IContainerBuilder builder)
        {
            if (registerViews) builder.RegisterComponentInHierarchy<BootErrorView>().As<IBootErrorView>();
            builder.RegisterEntryPoint<SceneUiBinding>();
            // cmpose:boot-steps:start
            // cmpose:boot-steps:end
            builder.RegisterEntryPoint<BootEntry>();
        }
    }
}
