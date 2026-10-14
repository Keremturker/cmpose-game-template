using System.Collections.Generic;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Network
{
    // network flag: IApiClient against ApiConfig's base URL (the debug one in development builds and the editor)
    public sealed class NetworkInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IHttpTransport, UnityWebRequestTransport>(Lifetime.Singleton);
            builder.Register<INetworkReachability, UnityNetworkReachability>(Lifetime.Singleton);
            builder.Register<IApiClient>(resolver => new ApiClient(resolver.Resolve<IHttpTransport>(),
                ApiConfig.BaseUrl(Debug.isDebugBuild), resolver.Resolve<IReadOnlyList<IRequestObserver>>(),
                resolver.Resolve<INetworkReachability>(), resolver.Resolve<ILog>()), Lifetime.Singleton);
            builder.Register<IRemoteImageLoader>(resolver => new RemoteImageLoader(new UnityWebRequestTextureSource(),
                resolver.Resolve<ILog>(), texture => Object.Destroy(texture)), Lifetime.Singleton);
        }
    }
}
