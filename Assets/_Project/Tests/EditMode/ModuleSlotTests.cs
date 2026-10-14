using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Sample.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class ModuleSlotTests
    {
        [Test]
        public void The_sample_view_is_a_resources_prefab()
        {
            Assert.That(Resources.Load<SampleView>(SampleView.ResourcePath), Is.Not.Null);
        }

        [Test]
        public void Spawn_puts_the_panel_under_the_slot()
        {
            var slot = new GameObject("slot", typeof(RectTransform)).GetComponent<RectTransform>();
            var binder = new UiBinder(new TableLocalizer(new FakeStringSource(), new FakeLog(), false),
                new FixedThemeSource(Resources.Load<Palette>("Theme/Light")));
            var view = SampleView.Spawn(slot, binder);
            Assert.That(view.transform.parent, Is.EqualTo(slot));
            Object.DestroyImmediate(slot.gameObject);
        }
    }
}
