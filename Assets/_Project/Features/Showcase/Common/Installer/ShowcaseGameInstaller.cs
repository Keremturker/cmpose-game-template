using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Showcase.Common;
using VContainer;
using VContainer.Unity;
// cmpose:ads
using Kturker.UnityBase.Core.Ads;
using Kturker.UnityBase.Features.Showcase.Common.Ads;
// /cmpose:ads
// cmpose:leaderboards
using Kturker.UnityBase.Features.Showcase.Common.Leaderboards;
// /cmpose:leaderboards
// cmpose:saveSystem
using Kturker.UnityBase.Features.Showcase.Common.Save;
// /cmpose:saveSystem

namespace Kturker.UnityBase.Features.Showcase.Installer
{
    // The showcase's shared parts in a game scene (the game registers IShowcaseRun); registerViews=false lets tests
    // supply fake views. Flag blocks add their usings only for flag namespaces (a using repeated in two blocks would
    // warn), so the flagless ones the blocks need are here.
    public sealed class ShowcaseGameInstaller : IInstaller
    {
        private readonly bool registerViews;

        public ShowcaseGameInstaller(bool registerViews) => this.registerViews = registerViews;

        public void Install(IContainerBuilder builder)
        {
            if (registerViews) builder.RegisterComponentInHierarchy<GameOverView>().As<IGameOverView>();
            builder.Register<BestScore>(Lifetime.Scoped);
            builder.RegisterEntryPoint<GameOverPresenter>();
            // cmpose:ads
            if (registerViews)
            {
                builder.Register<IContinueView>(resolver => ContinueView.Spawn(
                    UnityEngine.Object.FindAnyObjectByType<GameOverView>().ActionSlot, resolver.Resolve<UiBinder>()), Lifetime.Scoped);
            }
            builder.RegisterEntryPoint<ShowcaseContinuePresenter>(resolver => new ShowcaseContinuePresenter(
                    resolver.Resolve<IContinueView>(), resolver.Resolve<IShowcaseRun>(), resolver.Resolve<IAds>(),
                    resolver.TryResolve<IPremiumStatus>(out var premium) ? premium : null, resolver.Resolve<IUiFeedback>()),
                Lifetime.Scoped).AsSelf();
            // /cmpose:ads
            // cmpose:leaderboards
            builder.RegisterEntryPoint<ShowcaseScoreReporter>();
            // /cmpose:leaderboards
            // cmpose:saveSystem
            builder.Register<RunStats>(Lifetime.Scoped);
            builder.RegisterEntryPoint<RunStatsRecorder>();
            // /cmpose:saveSystem
        }
    }
}
