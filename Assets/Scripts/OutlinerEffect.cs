using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OutlineEffect : MonoBehaviour
{
    public Color outlineColor = Color.black;
    [Range(0.01f, 0.2f)]
    public float outlineWidth = 0.05f;

    private GameObject outlineObj;

    void Start()
    {

        outlineObj = new GameObject("OutlineMesh");
        outlineObj.transform.SetParent(transform, false);


        MeshFilter originalMeshFilter = GetComponent<MeshFilter>();
        MeshRenderer originalRenderer = GetComponent<MeshRenderer>();

        MeshFilter outlineMeshFilter = outlineObj.AddComponent<MeshFilter>();
        outlineMeshFilter.sharedMesh = originalMeshFilter.sharedMesh;

        MeshRenderer outlineRenderer = outlineObj.AddComponent<MeshRenderer>();


        Material outlineMat = new Material(Shader.Find("Unlit/Color"));
        outlineMat.color = outlineColor;

        outlineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);


        outlineMat.renderQueue = 1999;

        outlineRenderer.material = outlineMat;

        outlineObj.transform.localScale = Vector3.one * (1.0f + outlineWidth);
    }
}
