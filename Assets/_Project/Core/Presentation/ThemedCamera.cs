using UnityEngine;

namespace Kturker.UnityBase.Core.Presentation
{
    // Clears the camera with the palette's background; UiBinder binds it
    [RequireComponent(typeof(Camera))]
    public sealed class ThemedCamera : MonoBehaviour
    {
        private Camera target;
        private IThemeSource source;

        public void Bind(IThemeSource value)
        {
            if (source != value)
            {
                if (source != null) source.Changed -= Apply;
                source = value;
                if (source != null) source.Changed += Apply;
            }
            Apply();
        }

        private void OnDestroy()
        {
            if (source != null) source.Changed -= Apply;
        }

        private void Apply()
        {
            if (!this || source == null) return;
            if (target == null) target = GetComponent<Camera>();
            target.backgroundColor = source.Current.Get(ThemeRole.Background);
        }
    }
}
