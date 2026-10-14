using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kturker.UnityBase.Tests.EditMode
{
    // The showcase's layering (spec): a flag part references the common part and the core only (never another part,
    // the sample, the shell or the app); the common part references flagless code only; a game references the common
    // part and the core only (never another game, a flag part, the app, the sample or the shell); outside the showcase
    // only the app and the tests reference it, so the sample never does
    public sealed class ShowcaseArchitectureTests
    {
        private const string Root = "Kturker.UnityBase.";
        private const string Showcase = Root + "Features.Showcase.";
        private static readonly string[] Parts = { "Common.Ads", "Common.Purchases", "Common.Leaderboards", "Common.Save" };
        private static readonly string[] Games = { "Runner2D", "Track3D" };

        private static readonly string[] Flagless =
        {
            Root + "Core.Domain", Root + "Core.Navigation", Root + "Core.Presentation", Root + "Core.Preferences",
            Root + "Core.Localization", Root + "Core.Audio", Root + "Core.Haptics"
        };

        [System.Serializable]
        private sealed class Asmdef
        {
            public string name;
            public string[] references;
        }

        private static Dictionary<string, string[]> Assemblies()
        {
            var all = new Dictionary<string, string[]>();
            foreach (var guid in AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets/_Project" }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(AssetDatabase.GUIDToAssetPath(guid));
                var parsed = JsonUtility.FromJson<Asmdef>(asset.text);
                all[parsed.name] = parsed.references ?? new string[0];
            }
            return all;
        }

        // The game an assembly belongs to (the game, its Domain and its Tests), or null
        private static string GameOf(string assembly) =>
            Games.FirstOrDefault(g => assembly == Showcase + g || assembly.StartsWith(Showcase + g + "."));

        [Test]
        public void Flag_parts_reference_the_common_part_and_the_core_and_the_common_part_only_flagless_code()
        {
            var all = Assemblies();
            var problems = new List<string>();
            foreach (var part in Parts.Select(p => Showcase + p).Where(all.ContainsKey))
            {
                foreach (var reference in all[part])
                {
                    var forbidden = reference.StartsWith(Root + "App") || reference.StartsWith(Root + "Features.Sample")
                        || reference.StartsWith(Root + "Features.Shell")
                        || (reference.StartsWith(Showcase) && reference != Showcase + "Common");
                    if (forbidden) problems.Add($"{part} -> {reference}");
                }
            }
            foreach (var reference in all[Showcase + "Common"].Where(r => r.StartsWith(Root) && !Flagless.Contains(r)))
            {
                problems.Add($"{Showcase}Common -> {reference}");
            }
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Games_reference_the_common_part_and_the_core_never_each_other_a_flag_part_the_app_the_sample_or_the_shell()
        {
            var all = Assemblies();
            var games = all.Keys.Where(a => GameOf(a) != null).ToList();
            Assert.That(games, Is.Not.Empty);
            var problems = new List<string>();
            foreach (var assembly in games)
            {
                var own = GameOf(assembly);
                foreach (var reference in all[assembly])
                {
                    var forbidden = reference.StartsWith(Root + "App") || reference.StartsWith(Root + "Features.Sample")
                        || reference.StartsWith(Root + "Features.Shell")
                        || (reference.StartsWith(Showcase) && reference != Showcase + "Common" && GameOf(reference) != own);
                    if (forbidden) problems.Add($"{assembly} -> {reference}");
                }
            }
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Outside_the_showcase_only_the_app_and_the_tests_reference_it()
        {
            var problems = Assemblies()
                .Where(a => !a.Key.StartsWith(Showcase) && !a.Key.StartsWith(Root + "App") && !a.Key.StartsWith(Root + "Tests"))
                .SelectMany(a => a.Value.Where(r => r.StartsWith(Showcase)).Select(r => $"{a.Key} -> {r}"))
                .ToList();
            Assert.That(problems, Is.Empty);
        }
    }
}
