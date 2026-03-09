using UnityEngine;
using System.Collections;

public class RoomRevealTrigger : MonoBehaviour
{
    public Renderer darknessRenderer;
    public float fadeTime = 2f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(FadeDarkness());
        }
    }

    IEnumerator FadeDarkness()
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