using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Network;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.NetworkInspector
{
    // networkInspector flag (requires network): records IApiClient's exchanges, shows them in development builds
    public sealed class InspectorInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<InspectorObserver>(Lifetime.Singleton).As<IRequestObserver>().AsSelf();
            builder.RegisterEntryPoint<InspectorOverlayHost>(resolver => new InspectorOverlayHost(resolver.Resolve<InspectorObserver>(),
                resolver.Resolve<UiBinder>(), resolver.Resolve<ILog>(), Debug.isDebugBuild), Lifetime.Singleton);
        }
    }
}
