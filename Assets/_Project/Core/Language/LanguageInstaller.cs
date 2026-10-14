using Kturker.UnityBase.Core.Presentation;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Language
{
    // multiLang flag: picks the language when the root scope is built and adds the Language row to Settings
    public sealed class LanguageInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<ISystemLanguage, UnitySystemLanguage>(Lifetime.Singleton);
            builder.Register<LanguageSelector>(Lifetime.Singleton).As<ISettingsSection>().AsSelf();
            builder.RegisterBuildCallback(resolver => resolver.Resolve<LanguageSelector>().Apply());
        }
    }
}
