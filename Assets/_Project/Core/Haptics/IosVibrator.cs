#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;

namespace Kturker.UnityBase.Core.Haptics
{
    // UIImpactFeedbackGenerator through Assets/Plugins/iOS/CmposeHaptics.mm (simulators do not vibrate)
    public sealed class IosVibrator : IVibrator
    {
        [DllImport("__Internal")]
        private static extern void CmposeHapticImpact(int strength);

        public void Vibrate(HapticStrength strength) => CmposeHapticImpact((int)strength);
    }
}
#endif
