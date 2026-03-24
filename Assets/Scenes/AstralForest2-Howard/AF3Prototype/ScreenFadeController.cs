using System.Collections;
using UnityEngine;

public class ScreenFadeController : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup; // Black Screen UI
    public float fadeInDuration = 2.5f; // How long it takes to fade from black to visible

    public void FadeFromBlack()
    {
        StartCoroutine(FadeFromBlackRoutine());
    }

    IEnumerator FadeFromBlackRoutine() // If no canvas group, stop 
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.interactable = false; // Disables Interaction while fading
        fadeCanvasGroup.blocksRaycasts = false;

        float t = 0f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeInDuration); // Lerp from Black to transparent
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
    }
}