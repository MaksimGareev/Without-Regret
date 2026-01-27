using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenTransition : MonoBehaviour
{
    public RawImage fadeImage;
    public RectTransform creditsPanel;
    public float fadeDuration = 2f;
    public float scrollSpeed = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartTransition()
    {
        StartCoroutine(DoTransition());
    }

    private IEnumerator DoTransition()
    {
        // fade to black
        yield return StartCoroutine(FadeIn());

        // start scrolling credits
        StartCoroutine(ScrollCredits());
    }

    private IEnumerator FadeIn()
    {
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    private IEnumerator ScrollCredits()
    {
        while (true)
        {
            creditsPanel.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
