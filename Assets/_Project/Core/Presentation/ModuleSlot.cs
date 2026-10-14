using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Core.Presentation
{
    // The Game scene's vertical scroll where module panels stack; each module's installer spawns its panel into
    // Content. Every panel is as tall as the visible area (the viewport): one module fills the screen and does not
    // scroll, more modules scroll one screen each. The heights follow screen and orientation changes.
    public sealed class ModuleSlot : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform viewport;

        private float fittedHeight = -1f;
        private int fittedCount = -1;

        public RectTransform Content => content;

        // The loaded scene's slot Content, where installers spawn module panels; throws when no scene has a ModuleSlot
        // (the Game scene carries it), instead of a bare NullReferenceException inside the container
        public static RectTransform SceneContent()
        {
            var slot = FindAnyObjectByType<ModuleSlot>();
            if (slot == null)
            {
                throw new System.InvalidOperationException(
                    "No ModuleSlot in the loaded scenes: module panels spawn into the Game scene's ModuleSlot, so install modules from the Game scene");
            }
            return slot.Content;
        }

        // The visible area: the viewport, or Content's parent when the viewport is not wired; null when neither is
        private RectTransform Visible => viewport != null ? viewport : content != null ? content.parent as RectTransform : null;

        private void LateUpdate()
        {
            var visible = Visible;
            if (visible == null) return;
            if (Mathf.Approximately(visible.rect.height, fittedHeight) && content.childCount == fittedCount) return;
            Fit();
        }

        // Gives every panel under Content the visible area's height; does nothing without Content or a visible area
        public void Fit()
        {
            var visible = Visible;
            if (visible == null) return;
            var height = visible.rect.height;
            foreach (Transform panel in content)
            {
                var element = panel.GetComponent<LayoutElement>();
                if (element == null) element = panel.gameObject.AddComponent<LayoutElement>();
                element.minHeight = height;
                element.preferredHeight = height;
                element.flexibleHeight = 0f;
            }
            fittedHeight = height;
            fittedCount = content.childCount;
            LayoutRebuilder.MarkLayoutForRebuild(content);
        }
    }
}
