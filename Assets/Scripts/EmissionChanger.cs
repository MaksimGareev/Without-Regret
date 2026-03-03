using UnityEngine;

public class EmissionChanger : MonoBehaviour
{
    private Renderer r;
    private MaterialPropertyBlock mpb;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionIntensityID = Shader.PropertyToID("_EmissionIntensity");

    void Awake()
    {
        r = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void SetEmission(float intensity)
    {
        r.GetPropertyBlock(mpb);

        mpb.SetColor(EmissionColorID, Color.white); // assumes color is handled in shader
        mpb.SetFloat(EmissionIntensityID, intensity);

        r.SetPropertyBlock(mpb);
    }
}