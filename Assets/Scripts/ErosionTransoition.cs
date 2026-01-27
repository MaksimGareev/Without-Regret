using UnityEngine;

public class ErosionTransition : MonoBehaviour
{
    [Header("Erosion Settings")]
    [Range(0f, 1f)]
    public float minValue = 0.3f;

    [Range(0f, 1f)]
    public float maxValue = 0.6f;

    public float speed = 1f;

    private Material materialInstance;
    private float t;

    void Awake()
    {
        materialInstance = GetComponent<Renderer>().material;
    }

    void Update()
    {
        t += Time.deltaTime * speed;

        float erosion = Mathf.Lerp(minValue, maxValue, Mathf.PingPong(t, 1f));
        materialInstance.SetFloat("_ErosionAmount", erosion);
    }
}
