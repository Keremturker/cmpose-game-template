using System;
using VContainer.Unity;

namespace Kturker.UnityBase.Features.Showcase.Common.Save
{
    // Counts each finished run and its stars in a showcase game scene
    public sealed class RunStatsRecorder : IStartable, IDisposable
    {
        private readonly IShowcaseRun run;
        private readonly RunStats stats;

        public RunStatsRecorder(IShowcaseRun run, RunStats stats)
        {
            this.run = run;
            this.stats = stats;
        }

        public void Start() => run.Finished += OnFinished;

        public void Dispose() => run.Finished -= OnFinished;

        private void OnFinished(RunResult result) => stats.Record(result.Stars);
    }
}
