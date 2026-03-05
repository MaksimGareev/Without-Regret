using UnityEngine;

public class Echo_Spawn_Test : MonoBehaviour
{
    [Header("Spawn Shader")]
    public float duration = 2f; // Time to go from 0 to 1

    private Material materialInstance;
    private float timer = 0f;
    private bool isAnimating = false;
    private bool outlinePhase = true;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        materialInstance = renderer.material;

        // start at 0
        materialInstance.SetFloat("_NoiseAmnt", 0f);
        materialInstance.SetFloat("_Outline", 0f);
    }

    void Update()
    {
        // press 9 key to start animation
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            timer = 0f;
            isAnimating = true;
            outlinePhase = true;

            materialInstance.SetFloat("_NoiseAmnt", 0f);
            materialInstance.SetFloat("_Outline", 0f);
        }

        // update shader value over time
        if (isAnimating)
        {
            timer += Time.deltaTime;
            float value = Mathf.Clamp01(timer / duration);

            if (outlinePhase)
            {
                materialInstance.SetFloat("_Outline", value);

                // once outline reaches 1, switch to noise phase
                if (value >= 1f)
                {
                    outlinePhase = false;
                    timer = 0f; // reset timer for noise
                }
            }
            else
            {
                materialInstance.SetFloat("_NoiseAmnt", value);

                // stop animating once noise reaches 1
                if (value >= 1f)
                {
                    isAnimating = false;
                }
            }
        }
    }
}