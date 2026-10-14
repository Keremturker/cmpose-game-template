using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Shell.Ads
{
    // The menu banner for the ads flag in both project kinds (installed by RootInstaller's ads block)
    public sealed class ShellAdsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder) => builder.RegisterEntryPoint<MenuBanner>();
    }
}
