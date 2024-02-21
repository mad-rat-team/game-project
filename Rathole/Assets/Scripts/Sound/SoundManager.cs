using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private SoundEffectMapping soundEffectMapping;

    private static SoundManager sm;

    private Dictionary<SoundName, Sound> sounds = new();
    private AudioClipInfo currentSoundtrackInfo;
    private bool fadingOut;
    private float fadeOutDuration;
    private float fadeOutStartTime;

    private float sfxVolume = 0.5f;
    private float musicVolume = 0.5f;

    public static void PlaySoundEffect(SoundName soundName)
    {
        AudioClipInfo soundInfo = sm.sounds[soundName].GetAudioClipInfo();
        sm.sfxAudioSource.PlayOneShot(soundInfo.audioClip, soundInfo.volume);
    }

    public static void SetSoundtrack(SoundName soundName)
    {
        sm.currentSoundtrackInfo = sm.sounds[soundName].GetAudioClipInfo();
        sm.musicAudioSource.clip = sm.currentSoundtrackInfo.audioClip;
        sm.musicAudioSource.volume = sm.currentSoundtrackInfo.volume;
        sm.musicAudioSource.Play();
    }

    public static void SetSoundEffectVolume(float volume)
    {
        sm.sfxVolume = volume;
        sm.sfxMixer.SetFloat("Volume", SliderToDb(volume));
    }
    
    public static void SetMusicVolume(float volume)
    {
        sm.musicVolume = volume;
        sm.musicMixer.SetFloat("Volume", SliderToDb(volume));
    }

    private static float SliderToDb(float sliderVolume)
    {
        return Mathf.Log10(Mathf.Clamp(sliderVolume, Mathf.Epsilon, 1f)) * 20f + 10f;
    }

    public static void FadeOutSoundtrack(float fadeOutDuration)
    {
        sm.fadingOut = true;
        sm.fadeOutDuration = fadeOutDuration;
        sm.fadeOutStartTime = Time.unscaledTime;
    }

    public static void SavePrefs()
    {
        PlayerPrefs.SetFloat("SfxVolume", sm.sfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", sm.musicVolume);
    }

    public static void SaveSfxVolume()
    {
        PlayerPrefs.SetFloat("SfxVolume", sm.sfxVolume);
    }

    public static void SaveMusicVolume()
    {
        PlayerPrefs.SetFloat("MusicVolume", sm.musicVolume);
    }

    public static float GetSfxVolume()
    {
        return sm.sfxVolume;
    }
    public static float GetMusicVolume()
    {
        return sm.musicVolume;
    }

    private void LoadPrefs()
    {
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    }

    private void Awake()
    {
        if (sm != null)
        {
            Debug.LogWarning("More than 1 SoundManager in the scene");
            return;
        }
        sm = this;

        Dictionary<SoundName, List<AudioClipInfo>> tempDict = new();

        foreach (AudioClipInfo audioClipInfo in soundEffectMapping.audioClipInfoArray)
        {
            if (tempDict.ContainsKey(audioClipInfo.soundName)) {
                tempDict[audioClipInfo.soundName].Add(audioClipInfo);
            }
            else
            {
                tempDict.Add(audioClipInfo.soundName, new List<AudioClipInfo>() { audioClipInfo });
            }
        }

        foreach (var kv in tempDict)
        {
            if (kv.Value.Count == 1)
            {
                sounds[kv.Key] = new SimpleSound(kv.Value[0]);
            }
            else
            {
                // NOTE: Maybe there should be an option to allow repeating sounds
                sounds[kv.Key] = new RandomSound(kv.Value.ToArray(), false);
            }
        }

        LoadPrefs();
    }

    private void Update()
    {
        if (fadingOut)
        {
            float timePassedRelative = (Time.unscaledTime - fadeOutStartTime) / fadeOutDuration;
            musicAudioSource.volume = currentSoundtrackInfo.volume * (1 - timePassedRelative);
            if (timePassedRelative > 1f)
            {
                musicAudioSource.Stop();
                fadingOut = false;
            }
        }
    }
}
