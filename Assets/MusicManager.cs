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
        DontDestroyOnLoad(gameObject);

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

        if (sound.Source == null)
        {
            Debug.LogError("Unknown song name: " + songName);
            return;
        }

        sound.Source.clip = sound.GetClip();
        sound.Source.Play();
    }

    private void Start()
    {
        StartCoroutine(playEngineSound());
    }

    IEnumerator playEngineSound()
    {
        while (true)
        {
            Sound sound = Array.Find(sounds, s => s.Name == "BackgroundMusic");
            sound.Source.clip = sound.GetClip();
            sound.Source.Play();

            yield return new WaitForSeconds(sound.Source.clip.length + 5); // Delay
        }
    }
}
