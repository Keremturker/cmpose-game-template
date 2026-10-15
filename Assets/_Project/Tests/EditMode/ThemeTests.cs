using System;
using Kturker.UnityBase.Core.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Kturker.UnityBase.Tests.EditMode
{
    // WCAG contrast ratio of two opaque palette colours (sRGB values as the palette stores them): 1 = the same
    // luminance, 21 = black on white.
    // - Boxes stand out from what is behind them by BoxOnPanel: Track (the paywall's package rows on the Surface panel)
    //   from Surface; SurfaceVariant (the chosen row, the GitHub card on the Game screen) from Surface and Background.
    // - The chosen row stands out from the other rows by SelectedOnRow (it also shows a Primary side bar).
    // - Secondary text (TextMuted: the per-month detail) reaches SmallText on both row colours; accent text (Primary:
    //   the savings badge) reaches the palette's accent floor there.
    // - Primary parts (slider fill, checkbox mark) on Control, and Primary on Track, reach NonText (WCAG's 3:1 for
    //   user-interface components); Control on Surface reaches the palette's control floor.
    // The floors that differ: on a near-black Surface, Primary text cannot reach 3:1 on two row colours that are 1.10
    // apart while Primary also reaches 3:1 on Track, so dark asks 2.3 (it was 1.88 before); on a white Surface, a
    // Control dark enough for 3:1 against it would leave Primary under 3:1 on it, so light asks 1.4 (dark gets 3).
    public static class Contrast
    {
        public const float BoxOnPanel = 1.25f;
        public const float SelectedOnRow = 1.10f;
        public const float NonText = 3f;
        public const float SmallText = 3f;

        public static float Ratio(Color a, Color b)
        {
            var la = Luminance(a);
            var lb = Luminance(b);
            return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
        }

        private static float Channel(float v) => v <= 0.03928f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);

        private static float Luminance(Color c) => 0.2126f * Channel(c.r) + 0.7152f * Channel(c.g) + 0.0722f * Channel(c.b);

        public static void AssertBoxes(Palette palette, string name, float accentOnRow, float controlOnSurface)
        {
            var surface = palette.Get(ThemeRole.Surface);
            var primary = palette.Get(ThemeRole.Primary);
            var track = palette.Get(ThemeRole.Track);
            var variant = palette.Get(ThemeRole.SurfaceVariant);
            var control = palette.Get(ThemeRole.Control);
            Assert.That(Ratio(track, surface), Is.GreaterThanOrEqualTo(BoxOnPanel), $"{name}: track on surface");
            Assert.That(Ratio(variant, surface), Is.GreaterThanOrEqualTo(BoxOnPanel), $"{name}: surface variant on surface");
            Assert.That(Ratio(variant, palette.Get(ThemeRole.Background)), Is.GreaterThanOrEqualTo(BoxOnPanel), $"{name}: surface variant on background");
            Assert.That(Ratio(variant, track), Is.GreaterThanOrEqualTo(SelectedOnRow), $"{name}: chosen row on the others");
            foreach (var (row, rowName) in new[] { (track, "track"), (variant, "surface variant") })
            {
                Assert.That(Ratio(palette.Get(ThemeRole.TextMuted), row), Is.GreaterThanOrEqualTo(SmallText), $"{name}: muted text on {rowName}");
                Assert.That(Ratio(primary, row), Is.GreaterThanOrEqualTo(accentOnRow), $"{name}: primary text on {rowName}");
            }
            Assert.That(Ratio(primary, track), Is.GreaterThanOrEqualTo(NonText), $"{name}: primary on track");
            Assert.That(Ratio(primary, control), Is.GreaterThanOrEqualTo(NonText), $"{name}: primary on control");
            Assert.That(Ratio(control, surface), Is.GreaterThanOrEqualTo(controlOnSurface), $"{name}: control on surface");
        }
    }

    public sealed class ThemeTests
    {
        private static Palette Make(Color seed)
        {
            var palette = ScriptableObject.CreateInstance<Palette>();
            foreach (ThemeRole role in Enum.GetValues(typeof(ThemeRole)))
                palette.Set(role, new Color(seed.r, seed.g, (int)role / 10f, 1f));
            return palette;
        }

        [Test]
        public void A_palette_keeps_a_colour_per_role()
        {
            var palette = Make(Color.red);
            foreach (ThemeRole role in Enum.GetValues(typeof(ThemeRole)))
                Assert.That(palette.Get(role), Is.EqualTo(new Color(1f, 0f, (int)role / 10f, 1f)), role.ToString());
            Object.DestroyImmediate(palette);
        }

        [Test]
        public void The_light_palette_sets_boxes_apart_and_primary_on_track()
        {
            var light = AssetDatabase.LoadAssetAtPath<Palette>("Assets/_Project/Core/Presentation/Resources/Theme/Light.asset");
            Assert.That(light, Is.Not.Null);
            Contrast.AssertBoxes(light, "light", accentOnRow: 3f, controlOnSurface: 1.4f);
        }

        [Test]
        public void Themed_graphics_and_cameras_follow_the_source()
        {
            var light = Make(Color.white);
            var dark = Make(Color.black);
            var source = new FakeThemeSource { Current = light };
            var go = new GameObject("Button", typeof(RectTransform));
            var cameraGo = new GameObject("Camera", typeof(Camera));
            try
            {
                var image = go.AddComponent<Image>();
                var themed = go.AddComponent<ThemedGraphic>();
                var camera = cameraGo.GetComponent<Camera>();
                var themedCamera = cameraGo.AddComponent<ThemedCamera>();

                themed.Bind(source);
                themedCamera.Bind(source);
                Assert.That(themed.Role, Is.EqualTo(ThemeRole.Background));
                Assert.That(image.color, Is.EqualTo(light.Get(ThemeRole.Background)));
                Assert.That(camera.backgroundColor, Is.EqualTo(light.Get(ThemeRole.Background)));

                source.Switch(dark);
                Assert.That(image.color, Is.EqualTo(dark.Get(ThemeRole.Background)));
                Assert.That(camera.backgroundColor, Is.EqualTo(dark.Get(ThemeRole.Background)));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(cameraGo);
                Object.DestroyImmediate(light);
                Object.DestroyImmediate(dark);
            }
        }
    }
}
