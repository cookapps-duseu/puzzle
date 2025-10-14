using Lofelt.NiceVibrations;

namespace Template
{
    public struct HapticConstant
    {
        public float Amplitude;
        public float Frequency;
        public float Duration;
    }
    public struct HapticEmphasis
    {
        public float Amplitude;
        public float Frequency;
    }

    public static class HapticPatternsHelper
    {
        public static void PlayHaptic(HapticConstant haptic)
        {
            HapticPatterns.PlayConstant(haptic.Amplitude, haptic.Frequency, haptic.Duration);
        }

        public static void PlayHaptic(HapticEmphasis haptic)
        {
            HapticPatterns.PlayEmphasis(haptic.Amplitude, haptic.Frequency);
        }

        private static HapticConstant soft = new HapticConstant
        {
            Amplitude = 0.4f,
            Frequency = 1f,
            Duration = 0.005f
        };
        public static void PlaySoftHaptic()
        {
            if (!SettingOptions.IsVibrate)
                return;

            PlayHaptic(soft);
        }

        private static HapticConstant hard = new HapticConstant
        {
            Amplitude = 0.6f,
            Frequency = 1f,
            Duration = 0.005f
        };
        public static void PlayHardHaptic()
        {
            if (!SettingOptions.IsVibrate)
                return;

            PlayHaptic(hard);
        }
    }
}
