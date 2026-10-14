using System;
using System.Collections.Generic;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Tests.EditMode
{
    // SceneBuilder is the only writer of scenes and prefabs; these checks keep its output honest
    public sealed class UiCoverageTests
    {
        private static readonly string[] Prefabs =
        {
            "Assets/_Project/Core/DI/ProjectLifetimeScope.prefab",
            "Assets/_Project/App/Settings/SettingsPanel.prefab",
            // cmpose:sample
            "Assets/_Project/Features/Sample/Resources/Sample/SampleView.prefab",
            // /cmpose:sample
            // cmpose:network
            // cmpose:sample
            "Assets/_Project/Features/Sample/Network/Resources/SampleNetwork/GitHubUserCard.prefab",
            // /cmpose:sample
            // /cmpose:network
            // cmpose:networkInspector
            "Assets/_Project/Core/NetworkInspector/Resources/NetworkInspector/InspectorOverlay.prefab",
            // /cmpose:networkInspector
            // cmpose:purchases
            "Assets/_Project/Features/Shell/Purchases/Resources/ShellPurchases/Paywall.prefab",
            // /cmpose:purchases
            // cmpose:ads
            // cmpose:sample
            "Assets/_Project/Features/Sample/Ads/Resources/SampleAds/Bonus.prefab",
            // /cmpose:sample
            // /cmpose:ads
            // cmpose:leaderboards
            // cmpose:sample
            "Assets/_Project/Features/Sample/Leaderboards/Resources/SampleLeaderboards/Scores.prefab",
            // /cmpose:sample
            // /cmpose:leaderboards
            // cmpose:showcase
            "Assets/_Project/Features/Showcase/Common/Resources/ShowcaseCommon/MenuBest.prefab",
            // cmpose:ads
            "Assets/_Project/Features/Showcase/Common/Ads/Resources/ShowcaseAds/Continue.prefab",
            // /cmpose:ads
            // cmpose:purchases
            "Assets/_Project/Features/Showcase/Common/Purchases/Resources/ShowcasePurchases/Premium.prefab",
            // /cmpose:purchases
            // cmpose:leaderboards
            "Assets/_Project/Features/Showcase/Common/Leaderboards/Resources/ShowcaseLeaderboards/Scores.prefab",
            // /cmpose:leaderboards
            // cmpose:saveSystem
            "Assets/_Project/Features/Showcase/Common/Save/Resources/ShowcaseSave/Stats.prefab",
            // /cmpose:saveSystem
            // /cmpose:showcase
        };

        private static readonly string[] Scenes =
        {
            "Assets/_Project/Scenes/Boot.unity",
            "Assets/_Project/Scenes/Menu.unity",
            // cmpose:sample
            "Assets/_Project/Scenes/Game.unity",
            // /cmpose:sample
            // cmpose:runner2D
            "Assets/_Project/Features/Showcase/Runner2D/Scenes/Runner2D.unity",
            // /cmpose:runner2D
            // cmpose:track3D
            "Assets/_Project/Features/Showcase/Track3D/Scenes/Track3D.unity",
            // /cmpose:track3D
        };

        // Screens whose content must keep clear of the screen's sides (a 20:9 phone shows about 57 fewer units each
        // side), with the least margin on the 1080 reference: Settings keeps its 90-unit column
        private static readonly (string Path, float Margin)[] EdgePrefabs =
        {
            ("Assets/_Project/App/Settings/SettingsPanel.prefab", 85f),
            // cmpose:networkInspector
            ("Assets/_Project/Core/NetworkInspector/Resources/NetworkInspector/InspectorOverlay.prefab", 48f),
            // /cmpose:networkInspector
            // cmpose:purchases
            ("Assets/_Project/Features/Shell/Purchases/Resources/ShellPurchases/Paywall.prefab", 48f),
            // /cmpose:purchases
        };

        private const float ReferenceWidth = 1080f;

        private static void ForEachRoot(Action<string, GameObject> check)
        {
            foreach (var path in Prefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                check(path, prefab);
            }
            foreach (var path in Scenes)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    foreach (var root in scene.GetRootGameObjects()) check(path, root);
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static string PathOf(Transform t) => t.parent == null ? t.name : $"{PathOf(t.parent)}/{t.name}";

        [Test]
        public void Every_text_has_a_key_or_is_written_by_code_and_every_key_is_in_english()
        {
            var english = new TableLocalizer(new ResourcesStringSource(), new FakeLog(), false);
            var problems = new List<string>();
            ForEachRoot((where, root) =>
            {
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    var localized = text.GetComponent<LocalizedText>();
                    if (localized == null)
                    {
                        problems.Add($"{where}: {PathOf(text.transform)} has no LocalizedText");
                    }
                    else if (!string.IsNullOrEmpty(localized.Key) && english.Get(localized.Key) == localized.Key)
                    {
                        problems.Add($"{where}: {PathOf(text.transform)} key '{localized.Key}' is not in Strings/en");
                    }
                }
            });
            Assert.That(problems, Is.Empty);
        }

        // Horizontal rects on the 1080x1920 reference canvas, from anchors and offsets; a vertical layout group's
        // child is placed the way the group places it (hidden children too, so rows the presenter shows count). A rect
        // as wide as the screen is a full-screen layer (canvas, panel, safe area); everything else keeps the screen's
        // margin from both sides.
        private static void CheckSides(Transform t, float parentMin, float parentMax, float margin, string where, List<string> problems)
        {
            var group = t.GetComponent<VerticalLayoutGroup>();
            foreach (Transform child in t)
            {
                if (!(child is RectTransform rect)) continue;
                var width = parentMax - parentMin;
                float min, max;
                if (group != null)
                {
                    var innerMin = parentMin + group.padding.left;
                    var innerMax = parentMax - group.padding.right;
                    if (group.childControlWidth && group.childForceExpandWidth)
                    {
                        min = innerMin;
                        max = innerMax;
                    }
                    else
                    {
                        var size = rect.sizeDelta.x + (rect.anchorMax.x - rect.anchorMin.x) * width;
                        var column = (int)group.childAlignment % 3;
                        min = column == 0 ? innerMin : column == 2 ? innerMax - size : (innerMin + innerMax - size) / 2f;
                        max = min + size;
                    }
                }
                else
                {
                    min = parentMin + rect.anchorMin.x * width + rect.offsetMin.x;
                    max = parentMin + rect.anchorMax.x * width + rect.offsetMax.x;
                }
                var fullScreen = min <= 0.01f && max >= ReferenceWidth - 0.01f;
                if (!fullScreen && (min < margin - 0.01f || max > ReferenceWidth - margin + 0.01f))
                {
                    problems.Add($"{where}: {PathOf(child)} spans {min:0.#}..{max:0.#}");
                }
                CheckSides(child, min, max, margin, where, problems);
            }
        }

        [Test]
        public void Settings_inspector_and_paywall_keep_clear_of_the_screen_sides()
        {
            var problems = new List<string>();
            foreach (var (path, margin) in EdgePrefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                CheckSides(prefab.transform, 0f, ReferenceWidth, margin, path, problems);
            }
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Button_labels_shrink_to_fit_their_button()
        {
            var problems = new List<string>();
            ForEachRoot((where, root) =>
            {
                foreach (var button in root.GetComponentsInChildren<Button>(true))
                {
                    var label = button.transform.Find("Label");
                    if (label == null) continue;
                    var text = label.GetComponent<TMP_Text>();
                    if (text == null || !text.enableAutoSizing || text.fontSizeMin > text.fontSizeMax ||
                        text.textWrappingMode == TextWrappingModes.NoWrap || text.overflowMode == TextOverflowModes.Overflow)
                    {
                        problems.Add($"{where}: {PathOf(label)} does not shrink to fit");
                    }
                }
            });
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Every_graphic_has_a_theme_role_and_every_camera_follows_the_background()
        {
            var problems = new List<string>();
            ForEachRoot((where, root) =>
            {
                foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                {
                    if (graphic.GetComponent<ThemedGraphic>() == null) problems.Add($"{where}: {PathOf(graphic.transform)} has no ThemedGraphic");
                }
                foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                {
                    if (camera.GetComponent<ThemedCamera>() == null) problems.Add($"{where}: {PathOf(camera.transform)} has no ThemedCamera");
                }
            });
            Assert.That(problems, Is.Empty);
        }
    }
}
