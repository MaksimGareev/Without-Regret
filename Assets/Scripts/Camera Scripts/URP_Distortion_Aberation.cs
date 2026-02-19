using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URP_Distortion_Aberation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 1f; 
    [SerializeField, UnityEngine.Min(0)] private float minIntensity = 0f;
    [SerializeField, UnityEngine.Min(0)] private float maxIntensity = 1f;

    [Header("Volume Reference")]
    [SerializeField] private Volume globalVolume; // Global Volume here

    private ChromaticAberration chromaticAberration; 
    private bool stop = false; 

    void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume not assigned!");
            return;
        }

        // check if ChromaticAberration exists in the URP Volume
        if (!globalVolume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration = globalVolume.profile.Add<ChromaticAberration>(true);
        }

        // make sure intensity override is enabled
        chromaticAberration.intensity.overrideState = true;

        // start the wobble/dizzyness effect
        TriggerDizzyness();
    }

    public void StopDizzyness()
    {
        stop = true; 
    }

    public void TriggerDizzyness()
    {
        stop = false; 
        StartCoroutine(DizzynessCycle()); 
    }

    IEnumerator DizzynessCycle()
    {
        int direction = 1; 

        while (!stop || (stop && chromaticAberration.intensity.value > minIntensity))
        {
            chromaticAberration.intensity.value += direction * speed * Time.deltaTime;

            if (chromaticAberration.intensity.value <= minIntensity)
            {
                chromaticAberration.intensity.value = minIntensity;
                direction = 1; 
            }
            else if (chromaticAberration.intensity.value >= maxIntensity)
            {
                chromaticAberration.intensity.value = maxIntensity;
                direction = -1; 
            }
            yield return null;
        }
    }
}
