using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Runner2D
{
    public sealed class Runner2DScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) => new Runner2DInstaller(registerViews: true).Install(builder);
    }
}
