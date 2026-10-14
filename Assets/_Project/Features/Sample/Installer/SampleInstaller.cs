using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Sample.Presentation;
using VContainer;
using VContainer.Unity;
// cmpose:network
using Kturker.UnityBase.Features.Sample.Network;
// /cmpose:network
// cmpose:ads
using Kturker.UnityBase.Features.Sample.Ads;
// /cmpose:ads
// cmpose:leaderboards
using Kturker.UnityBase.Features.Sample.Leaderboards;
// /cmpose:leaderboards

namespace Kturker.UnityBase.Features.Sample.Installer
{
    // registerViews=false lets tests supply fake views instead of spawning the panels
    public sealed class SampleInstaller : IInstaller
    {
        private readonly bool registerViews;

        public SampleInstaller(bool registerViews) => this.registerViews = registerViews;

        public void Install(IContainerBuilder builder)
        {
            // The panel is a Resources prefab spawned into the Game scene's ModuleSlot, so the scene names no module;
            // SceneContent fails with a clear message outside the Game scene
            if (registerViews) builder.Register(r => SampleView.Spawn(ModuleSlot.SceneContent(), r.Resolve<UiBinder>()), Lifetime.Scoped).AsSelf().As<ISampleView>();
            builder.Register<ICompletionCounter, NoCompletionCounter>(Lifetime.Singleton);
            // cmpose:saveSystem
            builder.Register<ICompletionCounter, SavedCompletionCounter>(Lifetime.Singleton);
            // /cmpose:saveSystem
            builder.RegisterEntryPoint<SamplePresenter>();
            // cmpose:network
            if (registerViews)
            {
                builder.Register<IGitHubUserCardView>(resolver => GitHubUserCardView.Spawn(
                    resolver.Resolve<SampleView>().ExtraSlot, resolver.Resolve<UiBinder>()), Lifetime.Scoped);
            }
            builder.RegisterEntryPoint<GitHubUserPresenter>();
            // /cmpose:network
            // cmpose:ads
            if (registerViews)
            {
                builder.Register<IBonusView>(resolver => BonusView.Spawn(
                    resolver.Resolve<SampleView>().ActionSlot, resolver.Resolve<UiBinder>()), Lifetime.Scoped);
            }
            builder.RegisterEntryPoint<SampleAdsPresenter>();
            // /cmpose:ads
            // cmpose:leaderboards
            if (registerViews)
            {
                builder.Register<ILeaderboardView>(resolver => LeaderboardView.Spawn(
                    resolver.Resolve<SampleView>().ScoreSlot, resolver.Resolve<UiBinder>()), Lifetime.Scoped);
            }
            builder.RegisterEntryPoint<SampleLeaderboardsPresenter>();
            // /cmpose:leaderboards
        }
    }
}
