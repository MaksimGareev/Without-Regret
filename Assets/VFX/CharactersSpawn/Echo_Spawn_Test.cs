using UnityEngine;
using System.Collections.Generic;

public class Echo_Spawn_Test : MonoBehaviour
{
    [Header("Spawn Shader")]
    public float duration = 2f; // Time to go from 0 to 1

    private Material materialInstance;
    private float timer = 0f;
    private bool isAnimating = false;
    private bool outlinePhase = true;
    private bool initialPhase = true;
    [SerializeField] List<SkinnedMeshRenderer> echoRenderers;
    [HideInInspector] public bool respawned = false;

    void Start()
    {

        // start at 0
        materialInstance.SetFloat("_NoiseAmnt", 0f);
        materialInstance.SetFloat("_Outline", 0f);
    }

    void Update()
    {
        if (respawned)
        {
            // initial set up
            if (initialPhase)
            {
                timer = 0f;
                isAnimating = true;
                outlinePhase = true;
                for (int i = 0; i < echoRenderers.Count; i++)
                {
                    materialInstance = echoRenderers[i].materials[1];
                    materialInstance.SetFloat("_NoiseAmnt", 0);
                    materialInstance.SetFloat("_Outline", 1f);
                }
                initialPhase = false;
            }

            // update shader value over time
            if (isAnimating)
            {
                timer += Time.deltaTime;
                float value = Mathf.Clamp01(timer / duration);

                if (outlinePhase)
                {
                    for (int i = 0; i < echoRenderers.Count; i++)
                    {
                        materialInstance = echoRenderers[i].materials[1];
                        materialInstance.SetFloat("_Outline", 1 - value);
                    }

                    // once outline reaches 1, switch to noise phase
                    if (value >= 1f)
                    {
                        outlinePhase = false;
                        timer = 0f; // reset timer for noise
                        isAnimating = false;
                        respawned = false;
                        initialPhase = true;
                    }
                }
            }
        }
    }
}