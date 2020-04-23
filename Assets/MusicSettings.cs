using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicSettings : MonoBehaviour
{
    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private Slider musicSlider;

    // Start is called before the first frame update
    void Start()
    {
        audioMixer.SetFloat("MusicVolume", 0);
        musicSlider.value = 1;
    }

    public void ToggleMusic()
    {
    }

    public void ToggleMusic(float value)
    {
        audioMixer.SetFloat("MusicVolume", (1 - value) * -80f);
    }

    public void ToggleSFX(float value)
    {
        audioMixer.SetFloat("SFXVolume", (1 - value) * -80f);
    }
}
