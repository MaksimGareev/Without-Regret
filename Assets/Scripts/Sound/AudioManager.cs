using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource CurrentMusic;
    public AudioSource NextMusic;

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
}
