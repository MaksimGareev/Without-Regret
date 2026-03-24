using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Audio;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("The canvas group found as a child of the SceneLoadManager prefab.")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Tooltip("The slider that will be updated to show the loading progress.")]
    [SerializeField] private Slider loadingProgressSlider;

    [Tooltip("Add any images that should be a loading screen. These will be randomly selected on loading.")]
    [SerializeField] private Image[] loadingImages;
    
    [Header("Settings")]
    [Tooltip("The duration of the fade in and fade out animations.")]
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private float audioFadeDuration = 0.5f;
    [SerializeField] private AudioMixer Mixer;

    float startVolume;
    float currentTime = 0;

    [HideInInspector] public UnityEvent OnSceneLoaded = new();

    private void Awake()
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

        if (canvasGroup == null)
        {
            Debug.LogError("Canvas group reference is missing in the SceneLoadManager script.");
        }
        else
        {
            canvasGroup.alpha = 0f;
        }

        if (loadingProgressSlider == null)
        {
            Debug.LogError("Slider reference is missing in the SceneLoadManager script.");
        }
        else
        {
            loadingProgressSlider.value = 0f;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        loadingProgressSlider.value = 0f;
        
        //Debug.Log("Fading in black screen");
        yield return FadeInBlackScreen();

        //Debug.Log("Starting scene load");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float displayedProgress = 0.0f;
        
        while (asyncLoad.progress < 0.9f)
        {
            float targetProgress = asyncLoad.progress / 0.9f;
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.unscaledDeltaTime * 2.0f);
            loadingProgressSlider.value = displayedProgress;
            
            yield return null;
        }

        while (displayedProgress < 1.0f)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, 1.0f, Time.unscaledDeltaTime * 2.0f);
            loadingProgressSlider.value = displayedProgress;
            yield return null;
        }

        //Debug.Log("Scene loaded");
        asyncLoad.allowSceneActivation = true;

        yield return new WaitForSecondsRealtime(0.5f);

        //Debug.Log("Syncing physics transforms");
        Physics.SyncTransforms();

        yield return new WaitForSecondsRealtime(0.1f);

        //Debug.Log("Invoking scene loaded event");
        StartCoroutine(InvokeSceneLoadedEvent());

        yield return WaitForStableFrameRate();

        //Debug.Log("Fading out black screen");
        yield return FadeOutBlackScreen();
    }

    private IEnumerator InvokeSceneLoadedEvent()
    {
        yield return null;
        OnSceneLoaded?.Invoke();
    }

    private IEnumerator WaitForStableFrameRate()
    {
        int stableFrameCount = 0;
        const int requiredStableFrames = 5;
        const float maxFrameTime = 1f / 10f; // Variable FPS threshold

        // Add a timeout to prevent infinite waiting in case of issues
        float timeOut = 5f;
        float timer = 0f;

        while (stableFrameCount < requiredStableFrames && timer < timeOut)
        {
            if (Time.unscaledDeltaTime < maxFrameTime)
            {
                stableFrameCount++;
            }
            else
            {
                stableFrameCount = 0; // Reset if we get a slow frame
            }

            timer += Time.unscaledDeltaTime;

            yield return null;
        }
    }

    private IEnumerator FadeInBlackScreen()
    {
        float startTime = Time.realtimeSinceStartup;
        float endTime = startTime + fadeDuration;

        StartCoroutine((FadeOutAudio()));

        while (Time.realtimeSinceStartup < endTime)
        {
            float t = Mathf.InverseLerp(startTime, endTime, Time.realtimeSinceStartup);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutBlackScreen()
    {
        float startTime = Time.realtimeSinceStartup;
        float endTime = startTime + fadeDuration;

        StartCoroutine((FadeInAudio()));

        while (Time.realtimeSinceStartup < endTime)
        {
            float t = Mathf.InverseLerp(startTime, endTime, Time.realtimeSinceStartup);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }

    private void OnDestroy()
    {
        Debug.Log("SceneLoadManager destroyed");
    }

    private IEnumerator FadeOutAudio()
    {
        Mixer.GetFloat("Master", out startVolume);
        currentTime = 0;
        while (currentTime <= fadeDuration)
        {
            currentTime += Time.deltaTime;
            Mixer.SetFloat("Master", Mathf.Lerp(startVolume, -80f, currentTime / fadeDuration));
            yield return null;
        }

    }

    private IEnumerator FadeInAudio()
    {
        currentTime = 0;
        while (currentTime <= fadeDuration)
        {
            currentTime += Time.deltaTime;
            Mixer.SetFloat("Master", Mathf.Lerp(-80f, startVolume, currentTime / fadeDuration));
            yield return null;
        }


    }
}
