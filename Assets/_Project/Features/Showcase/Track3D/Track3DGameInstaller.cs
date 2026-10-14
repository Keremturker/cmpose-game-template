using System;
using Kturker.UnityBase.Core.Audio;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Haptics;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Showcase.Track3D
{
    // The 3D track in its scene (Track3DScope installs it next to the shared parts); registerViews=false lets tests
    // supply fake views
    public sealed class Track3DGameInstaller : IInstaller
    {
        private readonly bool registerViews;

        public Track3DGameInstaller(bool registerViews) => this.registerViews = registerViews;

        public void Install(IContainerBuilder builder)
        {
            if (registerViews)
            {
                builder.RegisterComponentInHierarchy<TrackWorldView>().As<ITrackWorldView>();
                builder.RegisterComponentInHierarchy<ShowcaseHudView>().As<IShowcaseHudView>();
                builder.RegisterComponentInHierarchy<SwipeArea>().As<ITrackInput>();
            }
            builder.RegisterInstance(new TrackConfig());
            builder.Register<TrackSounds>(_ => TrackSounds.Create(), Lifetime.Scoped);
            builder.RegisterEntryPoint<Track3DPresenter>(resolver => new Track3DPresenter(
                    resolver.Resolve<ITrackWorldView>(), resolver.Resolve<IShowcaseHudView>(), resolver.Resolve<ITrackInput>(),
                    resolver.Resolve<TrackConfig>(), resolver.Resolve<TrackSounds>(), resolver.Resolve<IAudioPlayer>(),
                    resolver.Resolve<HapticsService>(), resolver.Resolve<ILog>(),
                    resolver.TryResolve<IPremiumStatus>(out var premium) ? premium : null, Environment.TickCount),
                Lifetime.Scoped).AsSelf();
        }
    }
}
