using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    private bool isFading = false;

    //Fade to black, run an action while black, then fade back in.
    public void StartFadeWithBlackEvent(System.Action onBlack, float blackHoldTime = 0.1f)
    {
        if (!isFading)
            StartCoroutine(FadeSequenceWithEvent(onBlack, blackHoldTime));
    }

    private IEnumerator FadeSequenceWithEvent(System.Action onBlack, float blackHoldTime)
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f));  //fade to black
        onBlack?.Invoke();                          //do stuff while black
        yield return new WaitForSeconds(blackHoldTime);
        yield return StartCoroutine(Fade(1f, 0f));  //fade back in

        isFading = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }
}