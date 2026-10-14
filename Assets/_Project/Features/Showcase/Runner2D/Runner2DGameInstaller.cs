using System;
using Kturker.UnityBase.Core.Audio;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Haptics;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Features.Showcase.Runner2D.Domain;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    // The 2D runner in its scene (Runner2DScope installs it next to the shared parts); registerViews=false lets tests
    // supply fake views
    public sealed class Runner2DGameInstaller : IInstaller
    {
        private readonly bool registerViews;

        public Runner2DGameInstaller(bool registerViews) => this.registerViews = registerViews;

        public void Install(IContainerBuilder builder)
        {
            if (registerViews)
            {
                builder.RegisterComponentInHierarchy<RunnerWorldView>().As<IRunnerWorldView>();
                builder.RegisterComponentInHierarchy<ShowcaseHudView>().As<IShowcaseHudView>();
                builder.RegisterComponentInHierarchy<TapArea>().As<ITapInput>();
            }
            builder.RegisterInstance(new RunnerConfig());
            builder.Register<RunnerSounds>(_ => RunnerSounds.Create(), Lifetime.Scoped);
            builder.RegisterEntryPoint<Runner2DPresenter>(resolver => new Runner2DPresenter(
                    resolver.Resolve<IRunnerWorldView>(), resolver.Resolve<IShowcaseHudView>(), resolver.Resolve<ITapInput>(),
                    resolver.Resolve<RunnerConfig>(), resolver.Resolve<RunnerSounds>(), resolver.Resolve<IAudioPlayer>(),
                    resolver.Resolve<HapticsService>(), resolver.Resolve<ILog>(),
                    resolver.TryResolve<IPremiumStatus>(out var premium) ? premium : null, Environment.TickCount),
                Lifetime.Scoped).AsSelf();
        }
    }
}
