using System.Globalization;
using System.Linq;
using System.Threading;
using Kturker.UnityBase.Core.Localization;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class TableLocalizerTests
    {
        [Test]
        public void Starts_in_english_and_merges_every_file_of_the_language()
        {
            var source = new FakeStringSource()
                .Add("en", "core", "{\"menu.play\":\"Play\"}")
                .Add("en", "sample", "{\"sample.greeting\":\"Hello from {0}\"}");
            var localizer = new TableLocalizer(source, new FakeLog(), true);

            Assert.That(localizer.Language, Is.EqualTo("en"));
            Assert.That(localizer.Get("menu.play"), Is.EqualTo("Play"));
            Assert.That(localizer.Get("sample.greeting", "Shop"), Is.EqualTo("Hello from Shop"));
        }

        [Test]
        public void A_missing_key_shows_the_key_and_warns_once_only_when_asked()
        {
            var source = new FakeStringSource().Add("en", "core", "{}");
            var log = new FakeLog();
            var localizer = new TableLocalizer(source, log, true);

            Assert.That(localizer.Get("nope"), Is.EqualTo("nope"));
            Assert.That(localizer.Get("nope"), Is.EqualTo("nope"));
            Assert.That(log.Lines.Count(l => l.StartsWith("WARNING Missing string 'nope'")), Is.EqualTo(1));
            Assert.That(localizer.Get(""), Is.Empty);
            Assert.That(localizer.Get(null), Is.Empty);

            var quiet = new FakeLog();
            new TableLocalizer(source, quiet, false).Get("nope");
            Assert.That(quiet.Lines, Is.Empty);
        }

        [Test]
        public void A_key_in_two_files_keeps_the_first_file_in_name_order()
        {
            var source = new FakeStringSource()
                .Add("en", "b", "{\"k\":\"from b\"}")
                .Add("en", "a", "{\"k\":\"from a\"}");
            var log = new FakeLog();
            var localizer = new TableLocalizer(source, log, false);

            Assert.That(localizer.Get("k"), Is.EqualTo("from a"));
            Assert.That(log.Lines, Has.Some.EqualTo("ERROR Strings/en/b: 'k' is already defined"));
        }

        [Test]
        public void Broken_files_and_values_that_are_not_text_are_skipped_with_an_error()
        {
            var source = new FakeStringSource()
                .Add("en", "x", "{not json")
                .Add("en", "y", "{\"n\":1,\"s\":\"S\"}")
                .Add("en", "z", "[\"array\"]");
            var log = new FakeLog();
            var localizer = new TableLocalizer(source, log, false);

            Assert.That(localizer.Get("s"), Is.EqualTo("S"));
            Assert.That(localizer.Get("n"), Is.EqualTo("n"));
            Assert.That(log.Lines, Is.EquivalentTo(new[]
            {
                "ERROR Strings/en/x: not a JSON object",
                "ERROR Strings/en/y: 'n' is not a string",
                "ERROR Strings/en/z: not a JSON object"
            }));
        }

        [Test]
        public void SetLanguage_switches_and_raises_Changed_and_an_unknown_language_changes_nothing()
        {
            var source = new FakeStringSource()
                .Add("en", "core", "{\"hi\":\"Hello\"}")
                .Add("tr", "core", "{\"hi\":\"Merhaba\"}");
            var log = new FakeLog();
            var localizer = new TableLocalizer(source, log, false);
            var changes = 0;
            localizer.Changed += () => changes++;

            Assert.That(localizer.SetLanguage("tr"), Is.True);
            Assert.That(localizer.Get("hi"), Is.EqualTo("Merhaba"));
            Assert.That(localizer.SetLanguage("xx"), Is.False);
            Assert.That(localizer.Language, Is.EqualTo("tr"));
            Assert.That(localizer.Get("hi"), Is.EqualTo("Merhaba"));
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(log.Lines, Has.Some.EqualTo("WARNING No strings for language 'xx'"));
        }

        [Test]
        public void Arguments_that_do_not_fit_return_the_text_and_text_without_arguments_is_not_formatted()
        {
            var source = new FakeStringSource().Add("en", "core", "{\"f\":\"{1} left\",\"b\":\"Use {braces}\"}");
            var log = new FakeLog();
            var localizer = new TableLocalizer(source, log, false);

            Assert.That(localizer.Get("f", "x"), Is.EqualTo("{1} left"));
            Assert.That(log.Lines, Has.Some.StartsWith("WARNING String 'f' for 'en' does not fit 1 argument(s)"));
            Assert.That(localizer.Get("b"), Is.EqualTo("Use {braces}"));
        }

        [Test]
        public void Numbers_format_the_same_in_every_culture()
        {
            var source = new FakeStringSource().Add("en", "core", "{\"n\":\"{0}\"}");
            var previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Assert.That(new TableLocalizer(source, new FakeLog(), false).Get("n", 1.5), Is.EqualTo("1.5"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }
    }
}
