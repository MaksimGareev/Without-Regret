using UnityEngine;

public class spawn_test : MonoBehaviour
{
    [Header("Spawn Shader Settings")]
    public float duration = 2f; // Time to go from 0 to 1

    private Material materialInstance;
    private float timer = 0f;
    private bool isAnimating = false;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        materialInstance = renderer.material;

        // start at 0
        materialInstance.SetFloat("_NoiseAmnt", 0f);
    }

    void Update()
    {
        // press 0 key to start animation
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            timer = 0f;
            isAnimating = true;
            materialInstance.SetFloat("_NoiseAmnt", 0f);
        }

        // update shader value over time
        if (isAnimating)
        {
            timer += Time.deltaTime;

            float value = Mathf.Clamp01(timer / duration);
            materialInstance.SetFloat("_NoiseAmnt", value);

            // stop animating once we reach 1
            if (value >= 1f)
            {
                isAnimating = false;

                // Destroy object after spawn finishes
                Destroy(gameObject);
            }
        }
    }
}