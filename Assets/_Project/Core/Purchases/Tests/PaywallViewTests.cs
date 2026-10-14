using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Shell.Purchases;
using NUnit.Framework;
using UnityEngine;

namespace Kturker.UnityBase.Core.Purchases.Tests
{
    // The paywall prefab (SceneBuilder): the chosen package stands out by its row colour and a wide side bar
    public sealed class PaywallViewTests
    {
        private PaywallView view;

        [SetUp]
        public void SetUp() => view = Object.Instantiate(Resources.Load<PaywallView>("ShellPurchases/Paywall"));

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(view.gameObject);

        private Transform Row(int i) => view.transform.Find($"Panel/SafeArea/Row{i}");

        private ThemeRole RoleOf(int i) => Row(i).GetComponent<ThemedGraphic>().Role;

        [Test]
        public void The_selected_row_takes_the_track_colour_and_the_others_the_background()
        {
            view.Select(1);
            for (var i = 0; i < PaywallView.Rows; i++)
            {
                Assert.That(RoleOf(i), Is.EqualTo(i == 1 ? ThemeRole.Track : ThemeRole.Background), $"row {i}");
                Assert.That(Row(i).Find("Selected").gameObject.activeSelf, Is.EqualTo(i == 1), $"row {i} bar");
            }
        }

        [Test]
        public void A_new_selection_returns_the_old_row_to_normal()
        {
            view.Select(1);
            view.Select(2);
            Assert.That(RoleOf(1), Is.EqualTo(ThemeRole.Background));
            Assert.That(RoleOf(2), Is.EqualTo(ThemeRole.Track));
            Assert.That(Row(1).Find("Selected").gameObject.activeSelf, Is.False);
        }

        [Test]
        public void The_selection_bar_is_wide_enough_to_see()
        {
            var bar = (RectTransform)Row(0).Find("Selected");
            Assert.That(bar.sizeDelta.x, Is.GreaterThanOrEqualTo(24f));
        }
    }
}
