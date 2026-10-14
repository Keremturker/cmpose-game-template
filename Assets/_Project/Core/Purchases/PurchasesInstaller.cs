using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Preferences;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Purchases
{
    // purchases flag: IPurchases over RevenueCat, configured only where PurchasesConfig.CanConfigure allows it
    public sealed class PurchasesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            var apiKey = PurchasesConfig.ApiKey(Application.platform == RuntimePlatform.IPhonePlayer);
            var available = PurchasesConfig.CanConfigure(apiKey, Debug.isDebugBuild, Application.isEditor);
            builder.Register<IRevenueCatGateway, RevenueCatGateway>(Lifetime.Singleton);
            builder.RegisterEntryPoint<RevenueCatPurchases>(resolver => new RevenueCatPurchases(resolver.Resolve<IRevenueCatGateway>(),
                resolver.Resolve<IPreferences>(), resolver.Resolve<ILog>(), apiKey, available), Lifetime.Singleton).AsSelf();
            builder.Register<ISettingsSection, PremiumSection>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FirstLaunchPaywall>();
        }
    }
}
