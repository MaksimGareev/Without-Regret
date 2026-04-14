using UnityEngine;
using System.Collections;

public class RoomRevealTrigger : MonoBehaviour
{
    public Renderer darknessRenderer;
    public float timeToFade = 2f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeDarkness(timeToFade));
            triggered = true;
        }
    }

    public IEnumerator FadeDarkness(float fadeTime)
    {
        if (!triggered)
        {
            Material mat = darknessRenderer.material;
            Color color = mat.color;

            float t = 0;

            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, t / fadeTime);

                mat.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            mat.color = new Color(color.r, color.g, color.b, 0);
        }
    }

    public IEnumerator FadeInDarkness(float fadeTime)
    {
        if (!triggered)
        {
            Material mat = darknessRenderer.material;
            Color color = mat.color;

            float t = 0;

            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, t / fadeTime);

                mat.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }
        }
    }


}
