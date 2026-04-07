using UnityEngine;
using System.Collections;

public class hideandshow : MonoBehaviour
{
    public GameObject ObjectToToggle;
    public RoomRevealTrigger[] DarknessZones;

    private void Awake()
    {
        foreach (Renderer r in ObjectToToggle.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                // Create a unique instance for runtime changes
                mats[i] = new Material(mats[i]);
            }
            r.materials = mats;
        }
        StartCoroutine(DissolveOut(.2f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine(DissolveIn(1f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine(DissolveOut(1f));
        }
    }

    IEnumerator DissolveIn(float duration)
    {
        Debug.Log("Fading out");
        Renderer[] renderers = ObjectToToggle.GetComponentsInChildren<Renderer>();
        float time = 0f;

        for (int c = 0; c < DarknessZones.Length; c++)
        {
            StartCoroutine(DarknessZones[c].FadeInDarkness(duration));
        }

        // store original colors
        Color[][] originalColors = new Color[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            originalColors[i] = new Color[mats.Length];
            for (int j = 0; j < mats.Length; j++)
            {
                originalColors[i][j] = mats[j].color;
            }
        }

        while (time < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, time / duration);

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    Color c = originalColors[i][j];
                    c.a = alpha;
                    mats[j].color = c;
                }
            }

            time += Time.deltaTime;
            yield return null;
        }
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                // Change surface type to transparent so alpha will work
                if (mats[i].HasProperty("_Surface"))
                {
                    mats[i].SetFloat("_Surface", 0f);
                }

                // Ensure rendering mode updates correctly
                mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mats[i].SetInt("_ZWrite", 1);
                mats[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mats[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }
        }


        foreach (Light l in ObjectToToggle.GetComponentsInChildren<Light>())
        {
            l.gameObject.SetActive(true);
        }

    }
    IEnumerator DissolveOut(float duration)
    {
        Debug.Log("Fading out");
        Renderer[] renderers = ObjectToToggle.GetComponentsInChildren<Renderer>();
        float time = 0f;

        for (int c = 0; c < DarknessZones.Length; c++)
        {
            StartCoroutine(DarknessZones[c].FadeDarkness(duration));
        }

        // switch materials to transparent to activate fade
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                // Change surface type to transparent so alpha will work
                if (mats[i].HasProperty("_Surface"))
                {
                    mats[i].SetFloat("_Surface", 1f);
                }

                // Ensure rendering mode updates correctly
                mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mats[i].SetInt("_ZWrite", 0);
                mats[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mats[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }

        // store original colors
        Color[][] originalColors = new Color[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            originalColors[i] = new Color[mats.Length];
            for (int j = 0; j < mats.Length; j++)
            {
                originalColors[i][j] = mats[j].color;
            }
        }

        while (time < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / duration);

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    Color c = originalColors[i][j];
                    c.a = alpha;
                    mats[j].color = c;
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        foreach (Light l in ObjectToToggle.GetComponentsInChildren<Light>())
        {
            l.gameObject.SetActive(false);
        }
    }
}
