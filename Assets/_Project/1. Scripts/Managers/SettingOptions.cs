using UnityEngine;

namespace Template
{
    public static class SettingOptions
    {
        public static void Initialize()
        {
            IsBGM = BgmPrefs;
            SoundManager.Instance.SetBGMVolume(IsBGM ? 1f : 0.001f);
            IsSFX = SfxPrefs;
            SoundManager.Instance.SetSFXVolume(IsSFX ? 1f : 0.001f);
            IsVibrate = VibratePrefs;
            IsAlarm = AlarmPrefs;
        }
    
        //설정
        public static bool IsBGM;
        public static bool IsSFX;
        public static bool IsVibrate;
        public static bool IsAlarm;

        public static bool BgmPrefs
        {
            get => PlayerPrefs.GetInt("BGM", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("BGM", value ? 1 : 0);
            
                SoundManager.Instance.SetBGMVolume(IsBGM ? 1f : 0.001f);
                IsBGM = value;
            }
        }

        public static bool SfxPrefs
        {
            get => PlayerPrefs.GetInt("SFX", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("SFX", value ? 1 : 0);
                SoundManager.Instance.SetSFXVolume(IsSFX ? 1f : 0.001f);
                IsSFX = value;
            }
        }

        public static bool VibratePrefs
        {
            get => PlayerPrefs.GetInt("VIBRATE", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("VIBRATE", value ? 1 : 0);
                IsVibrate = value;
            }
        }

        public static bool AlarmPrefs
        {
            get => PlayerPrefs.GetInt("ALARM", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("ALARM", value ? 1 : 0);
                IsAlarm = value;
            }
        }
    }
}
