using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct Sound
{
    public AudioClip AudioClip;

    public AudioClip[] other;

    public AudioClip GetClip()
    {
        if (other.Length > 0)
        {
            int choice = Random.Range(0, other.Length);
            return other[choice];
        }

        return AudioClip;
    }

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