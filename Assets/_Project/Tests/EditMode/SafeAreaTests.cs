using Kturker.UnityBase.Core.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class SafeAreaTests
    {
        [Test]
        public void Without_a_reservation_the_anchors_are_the_safe_area()
        {
            var (min, max) = SafeArea.Anchors(new Rect(0, 100, 1000, 1800), new Vector2Int(1000, 2000), 0f);

            Assert.That(min, Is.EqualTo(new Vector2(0f, 0.05f)));
            Assert.That(max, Is.EqualTo(new Vector2(1f, 0.95f)));
        }

        [Test]
        public void A_reserved_bottom_raises_the_content_but_never_above_its_top()
        {
            var screen = new Vector2Int(1000, 2000);

            var (raised, _) = SafeArea.Anchors(new Rect(0, 100, 1000, 1800), screen, 150f);
            var (tooHigh, top) = SafeArea.Anchors(new Rect(0, 100, 1000, 1800), screen, 5000f);

            Assert.That(raised.y, Is.EqualTo(0.125f));
            Assert.That(tooHigh.y, Is.EqualTo(top.y));
        }
    }
}
