using System;
using Kturker.UnityBase.Core.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Kturker.UnityBase.Tests.EditMode
{
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
