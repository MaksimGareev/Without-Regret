using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        yield return StartCoroutine(FadeInBlackScreen());
        //Time.timeScale = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return new WaitForSecondsRealtime(0.1f);

        OnSceneLoaded?.Invoke();
        Physics.SyncTransforms();

        yield return StartCoroutine(WaitForStableFrameRate());

        //Time.timeScale = 1f;
        yield return StartCoroutine(FadeOutBlackScreen());
    }

    private IEnumerator WaitForStableFrameRate()
    {
        int stableFrameCount = 0;
        const int requiredStableFrames = 5;
        const float maxFrameTime = 1f / 25f; // 25 FPS threshold

        while (stableFrameCount < requiredStableFrames)
        {
            if (Time.unscaledDeltaTime < maxFrameTime)
            {
                stableFrameCount++;
            }
            else
            {
                stableFrameCount = 0; // Reset if we get a slow frame
            }

            yield return null;
        }
    }

    private IEnumerator FadeInBlackScreen()
    {
        float elapsedTime = 0f;
        Color screenColor = blackScreen.color;
        Color textColor = loadingText.color;

        while (elapsedTime < fadeDuration)
        {
            screenColor.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            textColor.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            blackScreen.color = screenColor;
            loadingText.color = textColor;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        screenColor.a = 1f;
        textColor.a = 1f;
        loadingText.color = textColor;
        blackScreen.color = screenColor;
    }

    private IEnumerator FadeOutBlackScreen()
    {
        float elapsedTime = 0f;
        Color screenColor = blackScreen.color;
        Color textColor = loadingText.color;

        while (elapsedTime < fadeDuration)
        {
            screenColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            blackScreen.color = screenColor;
            loadingText.color = textColor;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        screenColor.a = 0f;
        textColor.a = 0f;
        loadingText.color = textColor;
        blackScreen.color = screenColor;
    }
    
}
