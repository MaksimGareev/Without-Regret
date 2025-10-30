using System.Collections.Generic;
using UnityEngine;

public class CameraSeeThrough : MonoBehaviour
{
    public Transform Player;            // Player reference
    public LayerMask obstructionMask;   // Layers that can block view
    public float radius = 0.15f;        // Circle radius (in screen space)
    public float softness = 0.05f;      // Edge softness

    private MaterialPropertyBlock propBlock;
    private Renderer currentRenderer;
    private bool PlayerHidden = false;

    public float fadeSpeed = 5f;

    private List<Renderer> currentObstructions = new List<Renderer>();
    private List<Material> fadedMaterials = new List<Material>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        propBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null) return;

        // Restore transparency for previous frame
        // RestoreObstructions();

        // Raycast from camera to player
        Vector3 Direction = Player.position - transform.position;
        float Distance = Vector3.Distance(transform.position, Player.position);
        
        // Check if something is between camera and player
        if (Physics.Raycast(transform.position, Direction, out RaycastHit hit, Distance, obstructionMask))
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                PlayerHidden = true;
                currentRenderer = renderer;

                // Conver player world position to screen position (0-1 UV space)
                Vector3 screenPos = Camera.main.WorldToViewportPoint(Player.position);

                renderer.GetPropertyBlock(propBlock);
                propBlock.SetVector("_CutoutPosition", new Vector4(screenPos.x, screenPos.y, 0, 0));
                propBlock.SetFloat("_CutoutRadius", radius);
                propBlock.SetFloat("_CutoutSoftness", softness);
                renderer.SetPropertyBlock(propBlock);
            }
        }
        else if (PlayerHidden && currentRenderer != null)
        {
            // Reset last obstructed renderer
            currentRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_CutoutVisible", 0f);
            currentRenderer.SetPropertyBlock(propBlock);
            PlayerHidden = false;
        }

    }

    private void RestoreObstructions()
    {
        if (fadedMaterials.Count > 0)
        {
            foreach (Material mat in fadedMaterials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = Mathf.MoveTowards(color.a, 1f, Time.deltaTime * fadeSpeed);
                    mat.color = color;
                    if (Mathf.Approximately(color.a, 1f))
                    {
                        mat.SetInt("_ZWrite", 1);
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    }
                }
            }
        }

        currentObstructions.Clear();
        fadedMaterials.Clear();
    }

}
