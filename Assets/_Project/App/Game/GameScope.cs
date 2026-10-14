using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Game
{
    public sealed class GameScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) => new GameInstaller(registerViews: true).Install(builder);
    }
}
