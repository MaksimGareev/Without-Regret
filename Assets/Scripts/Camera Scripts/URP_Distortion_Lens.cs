using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URP_Distortion_Lens : MonoBehaviour
{
    [Header("Wobble Settings")]
    // WOBBLYWIGGLYWOBBYWIGGILYWOBILLYWIGGILY OOOOOOOOOOOOOOOOoooooo WOAH WOAH SWUUBLE THAT WOBBILYWIGGILYWOBBILYWOBBILYWOBBILYWIGGILY🥀😭💀😭🥀😭💀😭🥀😭😭💀😭😭🥀😭😭
    [SerializeField] private float frequency = 1.0f;        // how often intensity changes
    [SerializeField] private float rigidness = 5.0f;        // how smoothly it moves
    [SerializeField] private float maxIntensity = 0.42f;    // max distortion amount
    [SerializeField] private Volume globalVolume;           // select global volume
    private float initialIntensity;                         // Save the initial value of the intensity for updating at runtime.

    private LensDistortion lens;
    private bool stop = true;

    void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume not assigned!");
            return;
        }

        // check if LensDistortion exists in the URP 
        if (!globalVolume.profile.TryGet(out lens))
        {
            lens = globalVolume.profile.Add<LensDistortion>(true);
        }

        // make sure intensity override is enabled
        lens.intensity.overrideState = true;

        if (lens.intensity.value == 0)
            lens.intensity.value = -10f;
        
        // Save the initial intensity value for later updates
        initialIntensity = maxIntensity;
        
        // Apply the intensity multiplier from settings and start the wobble 🥀😭💀😭🥀😭💀😭🥀😭😭💀😭😭🥀😭😭 effect
        UpdateIntensity(PlayerPrefs.GetInt("astralDistortion"));
    }

    public void StopWobble()
    {
        stop = true;
    }

    public void TriggerWobble()
    {
        stop = false;
        StartCoroutine(WobbleCycle());
    }

    IEnumerator WobbleCycle()
    {
        while (!stop)
        {
            float target = Random.Range(-maxIntensity, maxIntensity);
            float timer = Time.time + frequency;

            while (!stop && Time.time <= timer)
            {
                if (rigidness > 0)
                {
                    lens.intensity.value = Mathf.Lerp(
                        lens.intensity.value,
                        target,
                        rigidness * Time.deltaTime
                    );
                }
                else
                {
                    lens.intensity.value = target;
                }

                yield return null;
            }

            yield return null;
        }

        lens.intensity.value = 0f;
    }

    // Update max intensity based on the initial value and the new multiplier
    public void UpdateIntensity(int intensityMultiplier)
    {
        maxIntensity = initialIntensity * (intensityMultiplier /  100.0f);
        
        if (Mathf.Approximately(maxIntensity, 0f))
        {
            StopWobble();
        }
        else if (stop && !Mathf.Approximately(maxIntensity, 0f))
        {
            TriggerWobble();
        }
    }
}
