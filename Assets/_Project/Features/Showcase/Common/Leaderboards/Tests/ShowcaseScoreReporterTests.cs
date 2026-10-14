using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Leaderboards;
using Kturker.UnityBase.Core.Leaderboards.Tests;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Common.Leaderboards.Tests
{
    public sealed class ShowcaseScoreReporterTests
    {
        [Test]
        public void A_finished_run_reports_its_score_then_the_achievements()
        {
            var run = new FakeShowcaseRun();
            var services = new FakeGameServices();
            new ShowcaseScoreReporter(run, services).Start();

            run.Crash(new RunResult(42, 40, 0));
            run.Finish();

            Assert.That(services.Calls, Is.EqualTo(new[]
            {
                "score 42", $"unlock {AchievementKeys.FirstCompletion}", $"increment {AchievementKeys.TenCompletions} 1"
            }));
        }

        [Test]
        public void A_crash_alone_reports_nothing()
        {
            var run = new FakeShowcaseRun();
            var services = new FakeGameServices();
            new ShowcaseScoreReporter(run, services).Start();

            run.Crash(new RunResult(42, 40, 0));
            run.Continue();

            Assert.That(services.Calls, Is.Empty);
        }

        [Test]
        public void Leaving_the_scene_keeps_the_achievement_progress_of_a_finished_run()
        {
            var run = new FakeShowcaseRun();
            // FakeGameServices takes PendingScore at the call (a one-shot), so the test keeps its own handle
            var pending = new UniTaskCompletionSource<GameServicesResult>();
            var services = new FakeGameServices { PendingScore = pending };
            var reporter = new ShowcaseScoreReporter(run, services);
            reporter.Start();
            run.Crash(new RunResult(42, 40, 0));
            run.Finish();

            reporter.Dispose();
            pending.TrySetResult(GameServicesResult.Ok);

            Assert.That(services.Calls, Is.EqualTo(new[]
            {
                "score 42", $"unlock {AchievementKeys.FirstCompletion}", $"increment {AchievementKeys.TenCompletions} 1"
            }));
        }
    }
}
