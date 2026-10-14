using UnityEngine;

namespace Kturker.UnityBase.Core.Presentation
{
    // Keeps its RectTransform inside Screen.safeArea (notch, home indicator) and above ReservedBottom, and follows
    // rotation and reservation changes
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        // Pixels at the bottom of the screen that a native overlay covers (the ads banner); content stays above them
        public static float ReservedBottom { get; set; }

        private RectTransform rect;
        private Rect applied;
        private float reserved;
        private Vector2Int screen;

        private void Awake()
        {
            rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != applied || reserved != ReservedBottom || screen.x != Screen.width || screen.y != Screen.height) Apply();
        }

        // Screen-fraction anchors of the safe rect with its bottom raised by reservedBottom pixels (never above its top)
        public static (Vector2 Min, Vector2 Max) Anchors(Rect safe, Vector2Int screen, float reservedBottom)
        {
            var bottom = Mathf.Min(safe.yMin + Mathf.Max(0f, reservedBottom), safe.yMax);
            return (new Vector2(safe.xMin / screen.x, bottom / screen.y), new Vector2(safe.xMax / screen.x, safe.yMax / screen.y));
        }

        private void Apply()
        {
            applied = Screen.safeArea;
            reserved = ReservedBottom;
            screen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var (min, max) = Anchors(applied, screen, reserved);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
