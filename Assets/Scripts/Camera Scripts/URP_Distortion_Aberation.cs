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
    private float initialMaxIntensity; // Store the initial max intensity for updates
    private float initialMinIntensity; // Store the initial min intensity for updates
    private float intensityMultiplier = 1f;

    [Header("Volume Reference")]
    [SerializeField] private Volume globalVolume; // Global Volume here

    private ChromaticAberration chromaticAberration; 
    private bool stop = true;

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
        
        // Save initial settings for later updates
        initialMaxIntensity = maxIntensity;
        initialMinIntensity = minIntensity;

        // make sure intensity override is enabled
        chromaticAberration.intensity.overrideState = true;
        
        // Apply distortion intensity from settings
        UpdateIntensity(PlayerPrefs.GetInt("astralDistortion"));
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
            chromaticAberration.intensity.value += direction * intensityMultiplier * speed * Time.deltaTime;

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

    // Update max intensity based on the initial value and the new multiplier
    public void UpdateIntensity(int newIntensityMultiplier)
    {
        intensityMultiplier = newIntensityMultiplier / 100.0f;
        
        maxIntensity = initialMaxIntensity * intensityMultiplier;
        minIntensity = initialMinIntensity * intensityMultiplier;
        
        if (Mathf.Approximately(intensityMultiplier, 0f))
        {
            StopDizzyness();
        }
        else if (stop && !Mathf.Approximately(intensityMultiplier, 0f))
        {
            TriggerDizzyness();
        }
    }
}
