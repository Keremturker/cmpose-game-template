using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Boot
{
    public sealed class BootScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) => new BootInstaller(registerViews: true).Install(builder);
    }
}
