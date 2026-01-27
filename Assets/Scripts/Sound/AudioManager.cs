using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource CurrentMusic;
    public AudioSource NextMusic;

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer mixer;

    // Audio Mixer Groups
    private const string MasterGroup = "Master";
    private const string MusicGroup = "Music";
    private const string SFXGroup = "SFX";
    private const string DialogueGroup = "Dialogue";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAreaMusic(AudioClip newClip, float fadeTime)
    {
        if (CurrentMusic.clip == newClip) return;

        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(newClip, fadeTime));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        NextMusic.clip = newClip;
        NextMusic.Play();

        float time = 0;
        while (time < duration)
        {
            float t = time / duration;
            CurrentMusic.volume = Mathf.Lerp(1, 0, t);
            NextMusic.volume = Mathf.Lerp(0, 1, t);
            time += Time.deltaTime;
            yield return null;
        }

        CurrentMusic.Stop();

        // swap music
        (CurrentMusic, NextMusic) = (NextMusic, CurrentMusic);
    }

    public void SetMasterVolume(float volume)
    {
        ConvertVolumeToDecibels(MasterGroup, volume);
    }

    public void SetMusicVolume(float volume)
    {
        ConvertVolumeToDecibels(MusicGroup, volume);
    }

    public void SetSFXVolume(float volume)
    {
        ConvertVolumeToDecibels(SFXGroup, volume);
    }

    public void SetDialogueVolume(float volume)
    {
        ConvertVolumeToDecibels(DialogueGroup, volume);
    }

    private void ConvertVolumeToDecibels(string audioMixerGroup, float volume)
    {
        if (mixer == null) return;

        float dB;
        volume = Mathf.Clamp(volume, 0.0001f, 2f);

        if (volume > 0.0001f)
        {
            dB = 20f * Mathf.Log10(volume);
        }
        else
        {
            dB = -80f; // Minimum dB value
        }

        // Set the volume in the AudioMixer
        mixer.SetFloat(audioMixerGroup, dB);
    }
}
