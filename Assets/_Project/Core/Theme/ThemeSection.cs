using System;
using System.Collections.Generic;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Core.Theme
{
    // The Theme row in Settings: System, Light, Dark
    public sealed class ThemeSection : ISettingsSection
    {
        private static readonly ThemeMode[] Modes = { ThemeMode.System, ThemeMode.Light, ThemeMode.Dark };
        private static readonly SettingsOption[] Labels =
        {
            SettingsOption.Key("theme.system"), SettingsOption.Key("theme.light"), SettingsOption.Key("theme.dark")
        };

        private readonly SwitchableThemeSource theme;

        public ThemeSection(SwitchableThemeSource theme)
        {
            this.theme = theme;
            theme.Changed += OnThemeChanged;
        }

        public string TitleKey => "settings.theme";
        public IReadOnlyList<SettingsOption> Options => Labels;
        public int Selected => Array.IndexOf(Modes, theme.Mode);

        public event Action Changed;

        public void Select(int index)
        {
            if (index < 0 || index >= Modes.Length) return;
            theme.SetMode(Modes[index]);
        }

        private void OnThemeChanged() => Changed?.Invoke();
    }
}
