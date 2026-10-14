using System.Linq;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Kturker.UnityBase.Core.Language.Tests
{
    public sealed class LanguageSelectorTests
    {
        private sealed class FakeSystemLanguage : ISystemLanguage
        {
            public SystemLanguage Language { get; set; } = SystemLanguage.English;
        }

        private FakePreferences prefs;
        private FakeSystemLanguage system;
        private FakeLog log;
        private TableLocalizer localizer;
        private LanguageSelector selector;

        [SetUp]
        public void Create()
        {
            prefs = new FakePreferences();
            system = new FakeSystemLanguage();
            log = new FakeLog();
            localizer = new TableLocalizer(new FakeStringSource()
                .Add("en", "core", "{\"menu.play\":\"Play\",\"boot.error.step\":\"Could not start: {0}\"}")
                .Add("tr", "core", "{\"menu.play\":\"Oyna\",\"boot.error.step\":\"Başlatılamadı: {0}\"}")
                .Add("fr", "core", "{\"menu.play\":\"Jouer\",\"boot.error.step\":\"Impossible de démarrer : {0}\"}"), new FakeLog(), false);
            selector = new LanguageSelector(localizer, prefs, system, log);
        }

        [Test]
        public void The_system_language_decides_the_first_launch()
        {
            system.Language = SystemLanguage.Turkish;
            selector.Apply();

            Assert.That(localizer.Language, Is.EqualTo("tr"));
            Assert.That(selector.Selected, Is.EqualTo(1));
            Assert.That(log.Lines, Has.Some.EqualTo("MARKER CMPOSE_LANG code=tr"));
        }

        [Test]
        public void A_stored_choice_wins_over_the_system_language()
        {
            prefs.SetString(LanguageSelector.PreferenceKey, "fr");
            system.Language = SystemLanguage.Turkish;
            selector.Apply();

            Assert.That(localizer.Language, Is.EqualTo("fr"));
        }

        [Test]
        public void An_unknown_stored_or_system_language_falls_back()
        {
            prefs.SetString(LanguageSelector.PreferenceKey, "xx");
            system.Language = SystemLanguage.German;
            selector.Apply();
            Assert.That(localizer.Language, Is.EqualTo("en"));

            prefs.SetString(LanguageSelector.PreferenceKey, "");
            system.Language = SystemLanguage.Spanish;
            selector.Apply();
            Assert.That(localizer.Language, Is.EqualTo("en"));
            Assert.That(log.Lines.Last(), Is.EqualTo("MARKER CMPOSE_LANG code=en"));
        }

        [Test]
        public void Selecting_stores_switches_marks_and_raises_Changed()
        {
            selector.Apply();
            var changes = 0;
            selector.Changed += () => changes++;

            selector.Select(1);
            Assert.That(prefs.Values[LanguageSelector.PreferenceKey], Is.EqualTo("tr"));
            Assert.That(localizer.Language, Is.EqualTo("tr"));
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(log.Lines.Last(), Is.EqualTo("MARKER CMPOSE_LANG code=tr"));

            selector.Select(9);
            selector.Select(-1);
            Assert.That(localizer.Language, Is.EqualTo("tr"));
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void Options_are_the_native_names()
        {
            Assert.That(selector.TitleKey, Is.EqualTo("settings.language"));
            Assert.That(selector.Options.Select(o => o.Literal), Is.EqualTo(new[] { "English", "Türkçe", "Français", "Español", "Italiano" }));
        }

        [Test]
        public void Switching_rewrites_keyed_text_and_keeps_literals()
        {
            var keyedGo = new GameObject("Keyed", typeof(RectTransform));
            var literalGo = new GameObject("Literal", typeof(RectTransform));
            try
            {
                var keyedLabel = keyedGo.AddComponent<TextMeshProUGUI>();
                var keyed = keyedGo.AddComponent<LocalizedText>();
                var literalLabel = literalGo.AddComponent<TextMeshProUGUI>();
                var literal = literalGo.AddComponent<LocalizedText>();
                keyed.SetKey("boot.error.step", "audio");
                literal.SetLiteral("v1.0.0");
                keyed.Bind(localizer);
                literal.Bind(localizer);
                selector.Apply();
                Assert.That(keyedLabel.text, Is.EqualTo("Could not start: audio"));

                selector.Select(1);
                Assert.That(keyedLabel.text, Is.EqualTo("Başlatılamadı: audio"));
                Assert.That(literalLabel.text, Is.EqualTo("v1.0.0"));
            }
            finally
            {
                Object.DestroyImmediate(keyedGo);
                Object.DestroyImmediate(literalGo);
            }
        }
    }
}
