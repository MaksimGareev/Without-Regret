
using UnityEngine;

public class EmissionChanger : MonoBehaviour
{
    private Renderer r;
    private MaterialPropertyBlock mpb;

    private static readonly int EmissionID = Shader.PropertyToID("_EmissionIntensity");

    void Awake()
    {
        r = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void SetEmission(float intensity)
    {
        r.GetPropertyBlock(mpb);
        mpb.SetFloat(EmissionID, intensity);
        r.SetPropertyBlock(mpb);
    }
}