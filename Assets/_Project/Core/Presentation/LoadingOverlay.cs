using Kturker.UnityBase.Core.Navigation;
using UnityEngine;

namespace Kturker.UnityBase.Core.Presentation
{
    // Full-screen overlay on the root scope; blocks input while a scene loads
    public sealed class LoadingOverlay : MonoBehaviour, ILoadingOverlay
    {
        [SerializeField] private CanvasGroup group;

        private void Awake() => Set(false);

        public void Show() => Set(true);

        public void Hide() => Set(false);

        private void Set(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
        }
    }
}
