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
    [Tooltip("The black screen image that will be faded in and out.")]
    [SerializeField] private Image blackScreen;

    [Tooltip("Add any images that should be a loading screen. These will be randomly selected on loading.")]
    [SerializeField] private Image[] loadingImages;

    [Tooltip("The text that will be displayed during loading.")]
    [SerializeField] private TextMeshProUGUI loadingText;

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

        if (blackScreen == null)
        {
            Debug.LogError("Black screen image reference is missing in the SceneLoadManager script.");
        }
        else
        {
            // Ensure the black screen starts fully transparent
            Color screenColor = blackScreen.color;
            screenColor.a = 0f;
            blackScreen.color = screenColor;
        }

        if (loadingText == null)
        {
            Debug.LogError("Loading text reference is missing in the SceneLoadManager script.");
        }
        else
        {
            Color textColor = loadingText.color;
            textColor.a = 0f;
            loadingText.color = textColor;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        Debug.Log("Fading in black screen");
        yield return FadeInBlackScreen();

        
        Debug.Log("Starting scene load");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log("Scene loaded");
        asyncLoad.allowSceneActivation = true;

        yield return new WaitForSecondsRealtime(0.5f);

        Debug.Log("Syncing physics transforms");
        Physics.SyncTransforms();

        yield return new WaitForSecondsRealtime(0.1f);

        Debug.Log("Invoking scene loaded event");
        StartCoroutine(InvokeSceneLoadedEvent());

        yield return WaitForStableFrameRate();

        Debug.Log("Fading out black screen");
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

        Color screenColor = blackScreen.color;
        Color textColor = loadingText.color;

        StartCoroutine((FadeOutAudio()));

        while (Time.realtimeSinceStartup < endTime)
        {
            float t = Mathf.InverseLerp(startTime, endTime, Time.realtimeSinceStartup);

            screenColor.a = Mathf.Lerp(0f, 1f, t);
            textColor.a = Mathf.Lerp(0f, 1f, t);
            blackScreen.color = screenColor;
            loadingText.color = textColor;
            
            yield return null;
        }

        screenColor.a = 1f;
        textColor.a = 1f;
        loadingText.color = textColor;
        blackScreen.color = screenColor;
    }

    private IEnumerator FadeOutBlackScreen()
    {
        float startTime = Time.realtimeSinceStartup;
        float endTime = startTime + fadeDuration;

        Color screenColor = blackScreen.color;
        Color textColor = loadingText.color;

        StartCoroutine((FadeInAudio()));

        while (Time.realtimeSinceStartup < endTime)
        {
            float t = Mathf.InverseLerp(startTime, endTime, Time.realtimeSinceStartup);

            screenColor.a = Mathf.Lerp(1f, 0f, t);
            textColor.a = Mathf.Lerp(1f, 0f, t);
            blackScreen.color = screenColor;
            loadingText.color = textColor;
            
            yield return null;
        }

        screenColor.a = 0f;
        textColor.a = 0f;
        loadingText.color = textColor;
        blackScreen.color = screenColor;
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
