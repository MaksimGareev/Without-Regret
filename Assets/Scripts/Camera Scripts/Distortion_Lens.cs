using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Distortion_Lens: MonoBehaviour
{
    [Header("Wobble Settings")]
    //WOBBLYWIGGLYWOBBYWIGGILYWOBILLYWIGGILY OOOOOOOOOOOOOOOOoooooo WOAH WOAH SWUUBLE THAT WOBBILYWIGGILYWOBBILYWOBBILYWOBBILYWIGGILY🥀😭💀😭🥀😭💀😭🥀😭😭💀😭😭🥀😭😭
    [SerializeField] private float frequency;
    [SerializeField] private float rigidness;
    [SerializeField] private Vector2 maxValues;
    [SerializeField] private PostProcessVolume volume;

    private LensDistortion lens;
    private bool stop = false; 

    
    void Start()
    {
        //check if LensDistortion exists
        if (!volume.profile.TryGetSettings(out lens))
            lens = (LensDistortion)volume.profile.AddSettings(typeof(LensDistortion));

  
        lens.centerX.overrideState = true;
        lens.centerY.overrideState = true;

   
        if (lens.intensity.value == 0)
            lens.intensity.Override(-10); 

        // start the wobble🥀😭💀😭🥀😭💀😭🥀😭😭💀😭😭🥀😭😭 effect
        TriggerWobble();
    }

   
    public void StopWobble()
    {
        stop = true; 
    }

    // trigger the effect and start the cycle
    public void TriggerWobble()
    {
        stop = false; 
        StartCoroutine(WobbleCycle());
    }


    IEnumerator WobbleCycle()
    {
        while (!stop)
        {
            // generate new  positions
            Vector2 target;
            target.x = Random.Range(-maxValues.x, maxValues.x);
            target.y = Random.Range(-maxValues.y, maxValues.y);

            float timer = Time.time + frequency;

            while (!stop && Time.time <= timer)
            {
                if (rigidness > 0)
                {
                    lens.centerX.value = Mathf.Lerp(lens.centerX, target.x, rigidness * Time.deltaTime);
                    lens.centerY.value = Mathf.Lerp(lens.centerY, target.y, rigidness * Time.deltaTime);
                }
                else
                {
                    lens.centerX.value = target.x;
                    lens.centerY.value = target.y;
                }

                yield return null; 
            }
        }

        lens.centerX.value = 0;
        lens.centerY.value = 0;
    }
}
