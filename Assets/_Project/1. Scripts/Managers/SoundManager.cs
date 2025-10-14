using System.Collections.Generic;
using System.Linq;
using CS.AudioToolkit;
using RabbitDog;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    public enum eBgmPlayType
    {
        NORMAL,
        OVERRIDE
    }

    public enum eSoundMode
    {
        NORMAL,
        UI_INTENSE
    }

    public AudioMixer audioMixer;
    public List<AudioObject> listUIBgmStack = new();
    public static bool isLowPassFilterOn = false;
    public static bool IsSkipSound = false;
    public static string LastPlayedBGM = "";

    private static string[] bgmGroup = { "BGM" };
    private static string[] sfxGroup = { "SFX" };
    private static string[] voxGroup = { };

    public bool IsInitialized { get; private set; }

    public bool Initialize()
    {
        IsInitialized = true;
        return true;
    }

    internal static void PlayLobbyBGM(eBgmPlayType type = eBgmPlayType.NORMAL, float startTime = 0)
    {
        PlayBGM("bgm_lobby", type, startTime);
    }

    internal static void PlayInGameBGM(eBgmPlayType type = eBgmPlayType.NORMAL, float startTime = 0)
    {
        //LastPlayedBGM = $"bgm_ingame_0{Random.Range(1, 5)}";
        PlayBGM("bgm_ingame", type, startTime);
    }

    /// <summary>
    /// BGM 플레이
    /// </summary>
    internal static void PlayBGM(string bgm, eBgmPlayType type = eBgmPlayType.NORMAL, float startTime = 0)
    {
        if (AudioController.Instance == null)
        {
            return;
        }

        if (type != eBgmPlayType.OVERRIDE &&
            AudioController.GetAudioChannel(AudioChannelType.Music).currentlyPlaying != null &&
            AudioController.GetAudioChannel(AudioChannelType.Music).currentlyPlaying.audioID == bgm)
        {
            return; // 재생하려는 BGM이 재생 중인 BGM과 같을 경우 새로 재생하지 않고 재생을 유지한다.
        }

        StopBGM();
        AudioController.PlayMusic(bgm, AudioController.Instance.transform, startTime: startTime);
    }

    /// <summary>
    /// UI용 BGM 플레이
    /// 기존 일반 BGM은 잠시 중단
    /// </summary>
    internal static void PlayUIBGM(string bgm, eBgmPlayType type = eBgmPlayType.NORMAL)
    {
        if (AudioController.Instance == null)
        {
            return;
        }

        if (type != eBgmPlayType.OVERRIDE && AudioController.GetPlayingAudioObjects().Exists(x => x.audioID == bgm))
        {
            return; // 재생하려는 BGM이 재생 중인 BGM과 같을 경우 새로 재생하지 않고 재생을 유지한다.
        }

        foreach (var uiBgm in Instance.listUIBgmStack)
        {
            if (uiBgm == null) continue;

            uiBgm.Pause();
        }

        Instance.listUIBgmStack.Add(AudioController.Play(bgm, AudioController.Instance.transform));
    }

    /// <summary>
    /// 해당 BGM이 속해 있는 카테고리 반환
    /// </summary>
    internal static AudioCategory GetBGMCategory(string bgm)
    {
        if (AudioController.GetAudioItem(bgm) == null)
        {
            return null;
        }

        return AudioController.GetAudioItem(bgm).category;
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    internal static void StopBGM()
    {
        AudioController.StopChannel(AudioChannelType.Music);
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    internal static void StopBGM(string bgm)
    {
        AudioController.Stop(bgm);
    }

    /// <summary>
    /// 아웃게임 SFX 플레이
    /// 인게임에서는 실행 차단됨
    /// </summary>
    internal static void PlayLobbySFX(string sfx, float fixedPitch = 0)
    {
        // if (Facade.IsGameStart)
        // {
        //     return;
        // }

        PlaySFX(sfx, fixedPitch);
    }

    /// <summary>
    /// SFX 플레이
    /// </summary>
    internal static void PlaySFX(string sfx, float fixedPitch = 0)
    {
        if (IsSkipSound)
        {
            return;
        }

        if (AudioController.Instance == null || string.IsNullOrEmpty(sfx))
        {
            return;
        }

        var audioObject = AudioController.Play(sfx, AudioController.Instance.transform);
        if (audioObject != null)
        {
            if (fixedPitch != 0)
            {
                audioObject.pitch = fixedPitch;
            }
            else
            {
                //audioObject.pitch = Time.timeScale;
            }
        }
    }

    /// <summary>
    /// SFX 중단
    /// </summary>
    internal static void StopSFX(string sfx) // SFX 정지
    {
        AudioController.Stop(sfx);
    }

    /// <summary>
    /// 환경음 플레이
    /// </summary>
    internal static void PlayAMB(string sfx)
    {
        if (AudioController.Instance == null)
        {
            return;
        }

        StopAMB();
        AudioController.PlayAmbienceSound(sfx, AudioController.Instance.transform);
    }

    /// <summary>
    /// 환경음 중단
    /// </summary>
    internal static void StopAMB()
    {
        AudioController.StopChannel(AudioChannelType.Ambience);
    }

    internal void SetBGMVolume(float volume)
    {
        foreach (var str in bgmGroup)
        {
            AudioController.SetCategoryVolume(str, volume);
        }
    }

    internal void SetMasterVolume(float volume)
    {
        AudioController.SetGlobalVolume(volume);
    }

    internal void SetSFXVolume(float volume)
    {
        foreach (var str in sfxGroup)
        {
            AudioController.SetCategoryVolume(str, volume);
        }
    }

    internal void SetVoxVolume(float volume)
    {
        foreach (var str in voxGroup)
        {
            AudioController.SetCategoryVolume(str, volume);
        }
    }

    /// <summary>
    /// 클릭 사운드
    /// </summary>
    internal static void PlayClick()
    {
        if (AudioController.Instance == null)
        {
            return;
        }

        AudioController.Play("sfx_click", AudioController.Instance.transform);
    }

    /// <summary>
    /// 전체 사운드 정지
    /// </summary>
    internal static void StopAll()
    {
        var temp = bgmGroup.ToList();
        temp.AddRange(sfxGroup);
        temp.AddRange(voxGroup);

        foreach (var str in temp)
        {
            if (str != "BGM_UI")
            {
                AudioController.StopCategory(str);
            }
        }
    }
}
