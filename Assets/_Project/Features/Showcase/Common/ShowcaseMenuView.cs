using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    // A row in the menu's extra slot with the best score (prefab under Resources; SceneBuilder builds it)
    public sealed class ShowcaseMenuView : MonoBehaviour, IShowcaseMenuView
    {
        public const string ResourcePath = "ShowcaseCommon/MenuBest";

        [SerializeField] private LocalizedText best;

        public static ShowcaseMenuView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<ShowcaseMenuView>(ResourcePath);
            if (prefab == null) throw new InvalidOperationException($"missing Resources/{ResourcePath}");
            var view = Instantiate(prefab, slot, false);
            binder.BindTree(view.gameObject);
            return view;
        }

        public void ShowBest(int value) => best.SetKey(ShowcaseKeys.Best, value);
    }
}
