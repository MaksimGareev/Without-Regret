using UnityEngine;

public class spawn_test : MonoBehaviour
{
    [Header("Spawn Shader Settings")]
    public float duration = 1f; // Time to go from 0 to 1

    private Material materialInstance;
    private float timer = 0f;
    private bool isAnimating = false;
    private bool startSpawn = false;
    private bool noisePhase = false;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        materialInstance = renderer.material;
        startSpawn = true;

        // start at 0
        materialInstance.SetFloat("_Outline", 0f);
        materialInstance.SetFloat("_NoiseAmnt", 0f);
    }

    void Update()
    {
        // press 0 key to start animation
        if (startSpawn)
        {
            timer = 0f;
            isAnimating = true;
            materialInstance.SetFloat("_Outline", 0f);
            materialInstance.SetFloat("_NoiseAmnt", 0f);
            startSpawn = false;
        }

        // update shader value over time
        if (isAnimating)
        {
            timer += Time.deltaTime;

            float value = Mathf.Clamp01(timer / duration);
            materialInstance.SetFloat("_Outline", value);


            // stop animating once we reach 1
            if (value >= 1f)
            {
                isAnimating = false;
                noisePhase = true;
                timer = 0;
            }
        }
        if (noisePhase)
        {
            timer += Time.deltaTime;

            float value = Mathf.Clamp01(timer / duration);
            materialInstance.SetFloat("_NoiseAmnt", value);


            // stop animating once we reach 1
            if (value >= 1f)
            {
                noisePhase = false;
            }
}
    }
}