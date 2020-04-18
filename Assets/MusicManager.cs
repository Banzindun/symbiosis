using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{

    private static MusicManager instance;

    public static MusicManager Instance => instance;

    [SerializeField]
    Sound[] sounds;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < sounds.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            sounds[i].Source = source;
            source.clip = sounds[i].AudioClip;
            source.outputAudioMixerGroup = sounds[i].AudioMixerGroup;
            source.volume = sounds[i].Volume;
            source.pitch = sounds[i].Pitch;
            source.loop = sounds[i].Loop;
        }
    }
    public void Play(string songName)
    {
        Sound sound = Array.Find(sounds, s => s.Name == songName);
        sound.Source.Play();
    }

    private void Start()
    {
        Play("BackgroundMusic");
        Play("BackgroundEffect");
    }
}
