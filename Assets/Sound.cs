using System;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct Sound
{
    public AudioClip AudioClip;

    public AudioMixerGroup AudioMixerGroup;

    public string Name;

    [Range(0, 1f)]
    public float Volume;

    [Range(.1f, 3)]
    public float Pitch;

    public bool Loop;

    [HideInInspector]
    public AudioSource Source;

}