using UnityEngine;

[ExecuteAlways]
public class EyeAtlasController : MonoBehaviour
{
    [Header("Target Renderer & Material")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string emissionMapProperty = "_EmissionMap";

    [Header("Atlas Settings")]
    [SerializeField] private int columns = 2;
    [SerializeField] private int rows = 4;

    private Material _mat;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        _mat = targetRenderer.material;
    }

    /// <summary>
    /// Sets which eye (0–7) to display based on atlas position.
    /// </summary>
    public void SetEyeIndex(int index)
    {
        if (_mat == null) return;
        
        if (index < 0 || index >= columns * rows)
        {
            Debug.LogWarning($"Eye index {index} out of range!");
            return;
        }

        int col = index % columns;
        int row = index / columns;

        // Each cell’s UV offset
        Vector2 offset = new Vector2(
            col * (1f / columns),
            row * (1f / rows)
        );

        // Apply to emission texture
        _mat.SetTextureScale(emissionMapProperty, new Vector2(1f / columns, 1f / rows));
        _mat.SetTextureOffset(emissionMapProperty, offset);
    }
}