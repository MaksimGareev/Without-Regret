using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadManager : MonoBehaviour
{
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

    private void Awake()
    {
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
        Time.timeScale = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;
        yield return new WaitForSecondsRealtime(0.1f);

        Physics.SyncTransforms();

        while (FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length == 0)
        {
            yield return null;
        }

        Time.timeScale = 1f;
        yield return StartCoroutine(FadeOutBlackScreen());
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
            elapsedTime += Time.deltaTime;
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
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        screenColor.a = 0f;
        textColor.a = 0f;
        loadingText.color = textColor;
        blackScreen.color = screenColor;
    }
    
}
