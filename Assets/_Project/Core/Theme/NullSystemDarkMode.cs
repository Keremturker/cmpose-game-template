namespace Kturker.UnityBase.Core.Theme
{
    // Editor and other platforms: always light
    public sealed class NullSystemDarkMode : ISystemDarkMode
    {
        public bool IsDark => false;
    }
}
