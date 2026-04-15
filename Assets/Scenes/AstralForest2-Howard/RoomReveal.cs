using UnityEngine;
using System.Collections;

public class RoomRevealTrigger : MonoBehaviour
{
    public Renderer darknessRenderer;
    public float timeToFade = 2f;

    

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeDarkness(timeToFade));
           
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeInDarkness(timeToFade));
            
        }
    }

    public IEnumerator FadeDarkness(float fadeTime)
    {
        

        Material mat = darknessRenderer.material;
        Color color = mat.color;

        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 0, t / fadeTime);

            mat.color = new Color(color.r, color.g, color.b, alpha);

            yield return null;
        }

        mat.color = new Color(color.r, color.g, color.b, 0);
    }

    public IEnumerator FadeInDarkness(float fadeTime)
    {
        

        Material mat = darknessRenderer.material;
        Color color = mat.color;

        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 1, t / fadeTime);

            mat.color = new Color(color.r, color.g, color.b, alpha);

            yield return null;
        }

        mat.color = new Color(color.r, color.g, color.b, 1);
    }
}