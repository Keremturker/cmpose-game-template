using System;
using UnityEngine;

namespace Kturker.UnityBase.Core.Presentation
{
    // One colour per ThemeRole. The light palette is Resources/Theme/Light.asset (always there); the uiTheme flag
    // adds a dark one. SceneBuilder writes both.
    [CreateAssetMenu(menuName = "UnityBase/Palette", fileName = "Palette")]
    public sealed class Palette : ScriptableObject
    {
        [SerializeField] private Color background = Color.white;
        [SerializeField] private Color surface = Color.white;
        [SerializeField] private Color primary = Color.blue;
        [SerializeField] private Color onPrimary = Color.white;
        [SerializeField] private Color text = Color.black;
        [SerializeField] private Color textMuted = Color.gray;
        [SerializeField] private Color track = Color.gray;
        [SerializeField] private Color scrim = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color picture = Color.white;
        [SerializeField] private Color surfaceVariant = Color.gray;
        [SerializeField] private Color control = Color.gray;

        public Color Get(ThemeRole role)
        {
            switch (role)
            {
                case ThemeRole.Background: return background;
                case ThemeRole.Surface: return surface;
                case ThemeRole.Primary: return primary;
                case ThemeRole.OnPrimary: return onPrimary;
                case ThemeRole.Text: return text;
                case ThemeRole.TextMuted: return textMuted;
                case ThemeRole.Track: return track;
                case ThemeRole.Scrim: return scrim;
                case ThemeRole.Picture: return picture;
                case ThemeRole.SurfaceVariant: return surfaceVariant;
                case ThemeRole.Control: return control;
                default: throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        public void Set(ThemeRole role, Color color)
        {
            switch (role)
            {
                case ThemeRole.Background: background = color; break;
                case ThemeRole.Surface: surface = color; break;
                case ThemeRole.Primary: primary = color; break;
                case ThemeRole.OnPrimary: onPrimary = color; break;
                case ThemeRole.Text: text = color; break;
                case ThemeRole.TextMuted: textMuted = color; break;
                case ThemeRole.Track: track = color; break;
                case ThemeRole.Scrim: scrim = color; break;
                case ThemeRole.Picture: picture = color; break;
                case ThemeRole.SurfaceVariant: surfaceVariant = color; break;
                case ThemeRole.Control: control = color; break;
                default: throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }
    }
}
