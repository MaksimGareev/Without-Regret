using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class Brightness : MonoBehaviour
{
    public Slider brightnessSlider;
    public PostProcessProfile brightness;
    public PostProcessLayer layer;

    private AutoExposure exposure;
    public static float savedBrightness = 1f;

    void Start()
    {
        if (brightness.TryGetSettings(out exposure))
        {
            exposure.keyValue.value = Mathf.Max(0.05f, savedBrightness);

            if (brightnessSlider != null)
            {
                brightnessSlider.value = savedBrightness;
                brightnessSlider.onValueChanged.AddListener(AdjustBrightness);
            }
        }
    }

    public void AdjustBrightness(float value)
    {
        if (exposure != null)
        {
            float setValue = Mathf.Max(0.05f, value);
            exposure.keyValue.value = setValue;

            savedBrightness = setValue;
        }
    }
}
