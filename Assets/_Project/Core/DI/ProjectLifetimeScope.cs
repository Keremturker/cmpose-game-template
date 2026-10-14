using Kturker.UnityBase.Core.About;
using Kturker.UnityBase.Core.Audio;
using Kturker.UnityBase.Core.Lifecycle;
using Kturker.UnityBase.Core.Navigation;
using Kturker.UnityBase.Core.Preferences;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.DI
{
    // Root scope, created from VContainerSettings at startup and kept across scenes. Registers what lives on the
    // prefab (components, assets); plain services come from RootInstaller.
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingOverlay loadingOverlay;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private ApplicationFocusSource focusSource;
        [SerializeField] private AppConfig appConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(loadingOverlay).As<ILoadingOverlay>();
            builder.RegisterComponent(audioManager).As<IAudioPlayer>();
            builder.RegisterInstance(new UiSounds(clickSound));
            builder.RegisterInstance(appConfig);
            new RootInstaller().Install(builder);
            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<UiBinder>().BindTree(gameObject);
                audioManager.Bind(resolver.Resolve<AudioPreferences>());
                focusSource.Bind(resolver.Resolve<PauseService>(), resolver.Resolve<IPreferences>());
            });
        }
    }
}
