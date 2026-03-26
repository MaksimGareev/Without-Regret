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

    public void SetMasterVolume(int volume)
    {
        ConvertVolumeToDecibels(MasterGroup, volume);
    }

    public void SetMusicVolume(int volume)
    {
        ConvertVolumeToDecibels(MusicGroup, volume);
    }

    public void SetSFXVolume(int volume)
    {
        ConvertVolumeToDecibels(SFXGroup, volume);
    }

    public void SetDialogueVolume(int volume)
    {
        ConvertVolumeToDecibels(DialogueGroup, volume);
    }

    private void ConvertVolumeToDecibels(string audioMixerGroup, int volume)
    {
        if (mixer == null) return;

        // convert volume int to float
        float volumeFloat = volume / 100f;

        float dB;
        volumeFloat = Mathf.Clamp(volumeFloat, 0.0001f, 2f);

        if (volumeFloat > 0.0001f)
        {
            dB = 20f * Mathf.Log10(volumeFloat);
        }
        else
        {
            dB = -80f; // Minimum dB value
        }

        // Set the volume in the AudioMixer
        mixer.SetFloat(audioMixerGroup, dB);
    }
}
