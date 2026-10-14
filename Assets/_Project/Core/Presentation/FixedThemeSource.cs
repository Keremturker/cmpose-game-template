using System;

namespace Kturker.UnityBase.Core.Presentation
{
    // Without the uiTheme flag: the light palette, never changing
    public sealed class FixedThemeSource : IThemeSource
    {
        public FixedThemeSource(Palette palette) => Current = palette;

        public Palette Current { get; }

        public event Action Changed
        {
            add { }
            remove { }
        }
    }
}
