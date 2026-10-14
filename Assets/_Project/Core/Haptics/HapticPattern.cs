using System;

namespace Kturker.UnityBase.Core.Haptics
{
    // Android one-shot vibration per strength: duration in ms and amplitude (1-255)
    public static class HapticPattern
    {
        public static long DurationMs(HapticStrength strength)
        {
            switch (strength)
            {
                case HapticStrength.Light: return 10;
                case HapticStrength.Medium: return 40;
                case HapticStrength.Heavy: return 80;
                default: throw new ArgumentOutOfRangeException(nameof(strength), strength, null);
            }
        }

        public static int Amplitude(HapticStrength strength)
        {
            switch (strength)
            {
                case HapticStrength.Light: return 80;
                case HapticStrength.Medium: return 160;
                case HapticStrength.Heavy: return 255;
                default: throw new ArgumentOutOfRangeException(nameof(strength), strength, null);
            }
        }
    }
}
