using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Common.Save
{
    // A row in the menu's extra slot with the totals (SceneBuilder builds it)
    public sealed class RunStatsView : MonoBehaviour, IRunStatsView
    {
        public const string ResourcePath = "ShowcaseSave/Stats";

        [SerializeField] private LocalizedText stats;

        public static RunStatsView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<RunStatsView>(ResourcePath);
            if (prefab == null) throw new InvalidOperationException($"missing Resources/{ResourcePath}");
            var view = Instantiate(prefab, slot, false);
            binder.BindTree(view.gameObject);
            return view;
        }

        public void Show(int runs, int stars) => stats.SetKey(ShowcaseKeys.Stats, runs, stars);
    }
}
