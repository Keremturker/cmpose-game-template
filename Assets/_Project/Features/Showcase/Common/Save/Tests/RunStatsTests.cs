using System.Collections.Generic;
using Kturker.UnityBase.Core.Save;
using Kturker.UnityBase.Features.Showcase.Common;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Common.Save.Tests
{
    public sealed class RunStatsTests
    {
        private sealed class MemorySaveStore : ISaveStore
        {
            private readonly Dictionary<string, object> slots = new Dictionary<string, object>();
            public T Load<T>(SaveSchema<T> schema) where T : class => slots.TryGetValue(schema.Slot, out var data) ? (T)data : null;
            public void Save<T>(SaveSchema<T> schema, T data) where T : class => slots[schema.Slot] = data;
            public void Delete(string slot) => slots.Remove(slot);
        }

        [Test]
        public void Totals_start_at_zero_and_add_up_across_instances()
        {
            var store = new MemorySaveStore();
            Assert.That((new RunStats(store).Runs, new RunStats(store).Stars), Is.EqualTo((0, 0)));

            new RunStats(store).Record(3);
            new RunStats(store).Record(2);

            var stats = new RunStats(store);
            Assert.That((stats.Runs, stats.Stars), Is.EqualTo((2, 5)));
        }

        [Test]
        public void Broken_totals_read_as_zero()
        {
            var store = new MemorySaveStore();
            store.Save(RunStats.Schema, new RunStats.Totals { Runs = -4, Stars = -1 });

            var stats = new RunStats(store);

            Assert.That((stats.Runs, stats.Stars), Is.EqualTo((0, 0)));
        }

        [Test]
        public void The_recorder_counts_each_finished_run_once()
        {
            var store = new MemorySaveStore();
            var run = new FakeShowcaseRun();
            new RunStatsRecorder(run, new RunStats(store)).Start();

            run.Crash(new RunResult(30, 20, 1));
            run.Continue();
            run.Crash(new RunResult(55, 25, 3));
            run.Finish();
            run.Finish();

            var stats = new RunStats(store);
            Assert.That((stats.Runs, stats.Stars), Is.EqualTo((1, 3)));
        }

        [Test]
        public void The_menu_shows_the_totals()
        {
            var store = new MemorySaveStore();
            new RunStats(store).Record(4);
            var view = new FakeRunStatsView();

            new RunStatsPresenter(view, new RunStats(store)).Start();

            Assert.That((view.Runs, view.Stars), Is.EqualTo(((int?)1, (int?)4)));
        }
    }
}
