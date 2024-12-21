using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager Instance;

    private static AudioSource audioSoruce;
    private static SoundEffectLibrary soundEffectlibrabry;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            audioSoruce = GetComponent<AudioSource>();
            soundEffectlibrabry = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Play(string soundName)
    {
        AudioClip audioClip = soundEffectlibrabry.GetRandomClip(soundName);
        if(audioClip != null)
        {
            audioSoruce.PlayOneShot(audioClip);
        }
    }

    private void Start()
    {
        sfxSlider.onValueChanged.AddListener(delegate { OnValueChange(); });
    }

    public static void SetVolume(float volume)
    {
        audioSoruce.volume = volume;
    }

    public void OnValueChange()
    {
        SetVolume(sfxSlider.value);
    }
}
