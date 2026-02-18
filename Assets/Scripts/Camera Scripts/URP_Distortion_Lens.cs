using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URP_Distortion_Lens : MonoBehaviour
{
    [Header("Wobble Settings")]
    // WOBBLYWIGGLYWOBBYWIGGILYWOBILLYWIGGILY OOOOOOOOOOOOOOOOoooooo WOAH WOAH SWUUBLE THAT WOBBILYWIGGILYWOBBILYWOBBILYWOBBILYWIGGILY🥀😭💀😭🥀😭💀😭🥀😭😭💀😭😭🥀😭😭
    
    [SerializeField] private float frequency = 1f;      // how often intensity changes
    [SerializeField] private float rigidness = 5f;      // how smoothly it moves
    [SerializeField] private float maxIntensity = 30f;  // max distortion amount
    [SerializeField] private Volume globalVolume;       // select global volume

    private LensDistortion lens;
    private bool stop = false;

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

        // start the wobble 🥀😭💀😭🥀😭💀😭🥀😭😭💀😭😭🥀😭😭 effect
        TriggerWobble();
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
}
