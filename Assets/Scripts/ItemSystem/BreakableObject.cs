using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip breakingSound;
    [SerializeField] private float volume = 0.5f;
    private AudioSource audioSource;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs;

    private void Awake()
    {
        InitializeSounds();
    }

    public void Break()
    {
        PlaySound();

        Destroy(gameObject); // replace with more advanced implementation once discussed more
    }
    
    private void InitializeSounds()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void PlaySound()
    {
        if (breakingSound != null)
        {
            audioSource.PlayOneShot(breakingSound, volume);
            if (showDebugLogs)
            {
                Debug.Log($"{name}: Sound playing {breakingSound} at {volume} volume");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"{name}: No sound effect assigned!");
        }
    }
}
