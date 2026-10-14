using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Track3D
{
    public sealed class Track3DScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) => new Track3DInstaller(registerViews: true).Install(builder);
    }
}
