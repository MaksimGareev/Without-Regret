using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DistortionAberation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 1f; 
    [SerializeField, UnityEngine.Min(0)] private float minIntensity = 0f;
    [SerializeField, UnityEngine.Min(0)] private float maxIntensity = 1f;
    


    [Header("Volume Reference")]
    [SerializeField] private PostProcessVolume volume; 
    
    


    private ChromaticAberration chromaticAberration; 
    private bool stop = false; 

    void Start()
    {



        if (!volume.profile.TryGetSettings(out chromaticAberration))
        {
            chromaticAberration = (ChromaticAberration)volume.profile.AddSettings(typeof(ChromaticAberration));
        }


        chromaticAberration.intensity.overrideState = true;

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
