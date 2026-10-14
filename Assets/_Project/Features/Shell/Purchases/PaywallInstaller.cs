using Kturker.UnityBase.Core.Purchases;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Shell.Purchases
{
    // The paywall for the purchases flag in both project kinds (installed by RootInstaller's purchases block)
    public sealed class PaywallInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder) => builder.Register<IPaywall, PaywallHost>(Lifetime.Singleton);
    }
}
