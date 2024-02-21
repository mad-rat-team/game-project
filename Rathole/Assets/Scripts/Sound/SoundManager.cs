using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource soundtrackAudioSource;
    //[SerializeField] private AudioClipInfo[] audioClipInfoArray;
    [SerializeField] private SoundEffectMapping soundEffectMapping;

    private static SoundManager sm;

    private AudioSource sfxAudioSource;
    private Dictionary<SoundName, Sound> sounds = new();

    public static void PlaySoundEffect(SoundName soundName)
    {
        AudioClipInfo soundInfo = sm.sounds[soundName].GetAudioClipInfo();
        sm.sfxAudioSource.PlayOneShot(soundInfo.audioClip, soundInfo.volume);
    }

    public static void SetSoundtrack()
    {
        //sm.sfxAudioSource.Play
    }

    private void Awake()
    {
        if (sm != null)
        {
            Debug.LogWarning("More than 1 SoundManager in the scene");
            return;
        }
        sm = this;

        sfxAudioSource = GetComponent<AudioSource>();

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
    }
}
