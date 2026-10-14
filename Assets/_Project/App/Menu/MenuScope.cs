using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Menu
{
    public sealed class MenuScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) => new MenuInstaller(registerViews: true).Install(builder);
    }
}
