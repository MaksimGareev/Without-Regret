using UnityEngine;
using UnityEngine.SceneManagement;
public class EndingMusicSelector : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip badEnding;
    public AudioClip neutralEnding;
    public AudioClip goodEnding;

    public AudioSource source;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Play();
    }


    public void Play()
    {
        if (NewDialogueManager.Instance == null)
        {
            Debug.LogError("NewDialogueManager not found!");
            return;
        }

        if (source == null)
        {
            Debug.LogError("AudioSource is NULL!");
            return;
        }

        int morality = NewDialogueManager.Instance.playerMorality;

        AudioClip clip = GetClip(morality);

        if (clip == null)
        {
            Debug.LogError("Clip is NULL! Check inspector assignments.");
            return;
        }
        /*
        if (clip != null && source != null)
        {
            source.PlayOneShot(clip);
        }
        */
        source.clip = clip;
        source.Play();
    }

    public AudioClip GetClip(int morality)
    {
        if (morality < -5)
        {
            return badEnding;
        }
        else if (morality > 5)
        {
            return goodEnding;
        }
        else
        {
            return neutralEnding;
        }
    }

}
