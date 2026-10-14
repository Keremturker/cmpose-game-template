using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Core.Presentation
{
    // Colours the Graphic (Image, TMP text) on this object from its role; UiBinder binds it
    public sealed class ThemedGraphic : MonoBehaviour
    {
        [SerializeField] private ThemeRole role;
        private Graphic graphic;
        private IThemeSource source;

        public ThemeRole Role => role;

        // Changes the role at run time (a selected row, say) and recolours from the bound theme
        public void SetRole(ThemeRole value)
        {
            role = value;
            Apply();
        }

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
            if (graphic == null) graphic = GetComponent<Graphic>();
            graphic.color = source.Current.Get(role);
        }
    }
}
