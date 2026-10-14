namespace Kturker.UnityBase.Core.Theme
{
    public static class ThemeModes
    {
        public static bool IsDark(ThemeMode mode, bool systemDark) =>
            mode == ThemeMode.Dark || (mode == ThemeMode.System && systemDark);
    }
}
