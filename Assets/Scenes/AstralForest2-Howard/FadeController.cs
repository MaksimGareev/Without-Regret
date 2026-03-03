using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;
    public float waitTime = 1f;

    private bool hasFaded = false;

    public void StartFade()
    {
        if (!hasFaded)
        {
            hasFaded = true;
            StartCoroutine(FadeSequence());
        }
    }

    IEnumerator FadeSequence()
    {
        yield return StartCoroutine(Fade(0f, 1f)); //Fade In

        yield return new WaitForSeconds(waitTime); //wait

        yield return StartCoroutine(Fade(1f, 0f)); //Fade Out
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}