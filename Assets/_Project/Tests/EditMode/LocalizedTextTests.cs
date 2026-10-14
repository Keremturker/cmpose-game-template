using Kturker.UnityBase.Core.Localization;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class LocalizedTextTests
    {
        private GameObject go;
        private TextMeshProUGUI label;
        private LocalizedText localized;
        private TableLocalizer localizer;

        [SetUp]
        public void Create()
        {
            go = new GameObject("Text", typeof(RectTransform));
            label = go.AddComponent<TextMeshProUGUI>();
            localized = go.AddComponent<LocalizedText>();
            var source = new FakeStringSource()
                .Add("en", "core", "{\"update.required.title\":\"Update required\",\"boot.error.step\":\"Could not start: {0}\"}")
                .Add("tr", "core", "{\"update.required.title\":\"Güncelleme gerekli\",\"boot.error.step\":\"Başlatılamadı: {0}\"}");
            localizer = new TableLocalizer(source, new FakeLog(), false);
        }

        [TearDown]
        public void Destroy() => Object.DestroyImmediate(go);

        [Test]
        public void Bound_text_follows_the_key_the_arguments_and_the_language()
        {
            localized.SetKey("boot.error.step", "audio");
            localized.Bind(localizer);
            Assert.That(label.text, Is.EqualTo("Could not start: audio"));

            localizer.SetLanguage("tr");
            Assert.That(label.text, Is.EqualTo("Başlatılamadı: audio"));

            localized.SetKey("update.required.title");
            Assert.That(label.text, Is.EqualTo("Güncelleme gerekli"));

            localized.SetLiteral("v1.0.0");
            localizer.SetLanguage("en");
            Assert.That(label.text, Is.EqualTo("v1.0.0"));
        }

        [Test]
        public void An_empty_key_leaves_text_written_by_code_alone()
        {
            label.text = "UnityBase";
            localized.Bind(localizer);
            localizer.SetLanguage("tr");

            Assert.That(label.text, Is.EqualTo("UnityBase"));
            Assert.That(localized.Key, Is.Null.Or.Empty);
        }
    }
}
