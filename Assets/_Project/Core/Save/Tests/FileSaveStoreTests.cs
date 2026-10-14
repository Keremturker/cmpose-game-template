using System;
using System.IO;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;

namespace Kturker.UnityBase.Core.Save.Tests
{
    public sealed class FileSaveStoreTests
    {
        private const string Root = "/save";
        private static readonly string Main = Path.Combine(Root, "sample.json");
        private static readonly SaveSchema<Score> V1 = new SaveSchema<Score>("sample", 1);

        [Serializable]
        public sealed class Score
        {
            [SerializeField] private int points;
            [SerializeField] private int level;
            public int Points { get => points; set => points = value; }
            public int Level { get => level; set => level = value; }
        }

        private sealed class Rename : ISaveMigration
        {
            public int From => 1;
            public int To => 2;
            public string Migrate(string dataJson) => dataJson.Replace("\"score\":", "\"points\":");
        }

        private sealed class AddLevel : ISaveMigration
        {
            public int From => 2;
            public int To => 3;
            public string Migrate(string dataJson) => dataJson.TrimEnd('}') + ",\"level\":1}";
        }

        private sealed class Failing : ISaveMigration
        {
            public int From => 1;
            public int To => 2;
            public string Migrate(string dataJson) => throw new InvalidOperationException("broken");
        }

        private sealed class Backwards : ISaveMigration
        {
            public int From => 2;
            public int To => 1;
            public string Migrate(string dataJson) => dataJson;
        }

        private FakeFileSystem files;
        private FakeLog log;
        private FileSaveStore store;

        private static string Envelope(int version, string data) => $"{{\"version\":{version},\"data\":{data}}}";

        [SetUp]
        public void Create()
        {
            files = new FakeFileSystem();
            log = new FakeLog();
            store = new FileSaveStore(files, Root, log);
        }

        [Test]
        public void Saves_round_trip_through_a_temporary_file_and_keep_the_previous_one_as_backup()
        {
            store.Save(V1, new Score { Points = 5 });
            Assert.That(files.Ops, Is.EqualTo(new[] { "write sample.json.tmp", "move sample.json.tmp sample.json" }));

            files.Ops.Clear();
            store.Save(V1, new Score { Points = 6 });
            Assert.That(files.Ops, Is.EqualTo(new[] { "write sample.json.tmp", "move sample.json sample.json.bak", "move sample.json.tmp sample.json" }));

            files.Ops.Clear();
            store.Save(V1, new Score { Points = 7 });
            Assert.That(files.Ops, Is.EqualTo(new[]
            {
                "write sample.json.tmp", "delete sample.json.bak", "move sample.json sample.json.bak", "move sample.json.tmp sample.json"
            }));

            Assert.That(files.Files[Main], Does.StartWith("{\"version\":1,\"data\":{"));
            Assert.That(files.Files[Main + ".bak"], Does.Contain("\"points\":6"));
            Assert.That(store.Load(V1).Points, Is.EqualTo(7));
            Assert.That(log.Lines, Is.Empty);
        }

        [Test]
        public void A_crash_between_the_moves_loads_the_backup()
        {
            files.Files[Main + ".bak"] = Envelope(1, "{\"points\":4,\"level\":0}");

            Assert.That(store.Load(V1).Points, Is.EqualTo(4));
            Assert.That(log.Lines, Has.Some.EqualTo("WARNING Save 'sample' could not be read; loaded the backup"));
        }

        [Test]
        public void Garbage_or_empty_files_fall_back_then_start_fresh()
        {
            files.Files[Main] = "garbage";
            files.Files[Main + ".bak"] = Envelope(1, "{\"points\":3}");
            Assert.That(store.Load(V1).Points, Is.EqualTo(3));

            files.Files[Main] = "";
            files.Files[Main + ".bak"] = Envelope(1, "[1]");
            Assert.That(store.Load(V1), Is.Null);
            Assert.That(log.Lines, Has.Some.EqualTo("WARNING Save 'sample' is unreadable; starting fresh"));
        }

        [Test]
        public void Nothing_saved_reads_as_null_without_a_warning()
        {
            Assert.That(store.Load(V1), Is.Null);
            Assert.That(log.Lines, Is.Empty);
        }

        [Test]
        public void Older_saves_are_migrated_step_by_step()
        {
            var v3 = new SaveSchema<Score>("sample", 3, new Rename(), new AddLevel());
            files.Files[Main] = Envelope(1, "{\"score\":5}");

            var loaded = store.Load(v3);
            Assert.That(loaded.Points, Is.EqualTo(5));
            Assert.That(loaded.Level, Is.EqualTo(1));
        }

        [Test]
        public void A_newer_save_is_not_loaded_even_with_an_older_backup()
        {
            files.Files[Main] = Envelope(9, "{\"points\":9}");
            files.Files[Main + ".bak"] = Envelope(1, "{\"points\":1}");

            Assert.That(store.Load(V1), Is.Null);
            Assert.That(log.Lines, Has.Some.EqualTo("ERROR Save 'sample' is version 9, newer than this game's 1; not loaded"));
        }

        [Test]
        public void A_missing_or_failing_migration_is_an_error_and_nothing_loads()
        {
            files.Files[Main] = Envelope(1, "{\"points\":1}");
            Assert.That(store.Load(new SaveSchema<Score>("sample", 2)), Is.Null);
            Assert.That(log.Lines, Has.Some.EqualTo("ERROR Save 'sample': no migration from version 1"));

            Assert.That(store.Load(new SaveSchema<Score>("sample", 2, new Failing())), Is.Null);
            Assert.That(log.Lines, Has.Some.EqualTo("ERROR Save 'sample': migration from 1 failed"));
        }

        [Test]
        public void A_failed_write_keeps_the_previous_save()
        {
            store.Save(V1, new Score { Points = 1 });
            files.FailOn = "write sample.json.tmp";

            Assert.Throws<IOException>(() => store.Save(V1, new Score { Points = 2 }));
            files.FailOn = null;
            Assert.That(store.Load(V1).Points, Is.EqualTo(1));
        }

        [Test]
        public void Delete_removes_the_save_its_backup_and_a_left_over_temporary_file()
        {
            store.Save(V1, new Score { Points = 1 });
            store.Save(V1, new Score { Points = 2 });
            files.Files[Main + ".tmp"] = "x";

            store.Delete("sample");
            Assert.That(files.Files, Is.Empty);
        }

        [Test]
        public void Slot_names_schemas_and_data_are_checked()
        {
            foreach (var bad in new[] { "", "../x", "Sample", "a/b", new string('a', 65) })
                Assert.Throws<ArgumentException>(() => store.Load(new SaveSchema<Score>(bad, 1)), bad);
            Assert.Throws<ArgumentException>(() => store.Delete(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SaveSchema<Score>("x", 0));
            Assert.Throws<ArgumentException>(() => new SaveSchema<Score>("x", 2, new Backwards()));
            Assert.Throws<ArgumentNullException>(() => store.Save(V1, null));
        }
    }
}
