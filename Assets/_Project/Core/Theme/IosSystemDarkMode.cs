#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;

namespace Kturker.UnityBase.Core.Theme
{
    // UITraitCollection through Assets/Plugins/iOS/CmposeTheme.mm
    public sealed class IosSystemDarkMode : ISystemDarkMode
    {
        [DllImport("__Internal")]
        private static extern int CmposeSystemDarkMode();

        public bool IsDark => CmposeSystemDarkMode() == 1;
    }
}
#endif
