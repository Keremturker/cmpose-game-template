using System.IO;
using Kturker.UnityBase.Features.Sample.Presentation;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Save.Tests
{
    public sealed class SavedCompletionCounterTests
    {
        [Test]
        public void Counts_persist_across_counters()
        {
            var store = new FileSaveStore(new FakeFileSystem(), "/save", new FakeLog());
            var first = new SavedCompletionCounter(store);

            Assert.That(first.Current, Is.EqualTo(0));
            Assert.That(first.Record(), Is.EqualTo(1));
            Assert.That(first.Record(), Is.EqualTo(2));
            Assert.That(new SavedCompletionCounter(store).Current, Is.EqualTo(2));
        }

        [Test]
        public void A_negative_stored_count_reads_as_zero()
        {
            var files = new FakeFileSystem();
            files.Files[Path.Combine("/save", "sample.json")] = "{\"version\":1,\"data\":{\"completed\":-5}}";
            var counter = new SavedCompletionCounter(new FileSaveStore(files, "/save", new FakeLog()));

            Assert.That(counter.Current, Is.EqualTo(0));
            Assert.That(counter.Record(), Is.EqualTo(1));
        }
    }
}
