using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Features.Showcase.Common.Save
{
    public sealed class RunStatsPresenter : Presenter<IRunStatsView>
    {
        private readonly RunStats stats;

        public RunStatsPresenter(IRunStatsView view, RunStats stats) : base(view) => this.stats = stats;

        public override void Start() => View.Show(stats.Runs, stats.Stars);
    }
}
