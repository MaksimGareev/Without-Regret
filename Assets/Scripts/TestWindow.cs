using UnityEngine;

public class TestWindow : MonoBehaviour
{
    public Material targetMaterial; // drag the material

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            targetMaterial.SetFloat("_EmissionIntensity", 0f);
        }
    }
}
