using UnityEngine;
using System.Collections;

public class TODManager : MonoBehaviour
{
    // sun and skybox references
    public Light directionalLight;
    public Material Skybox_Morning;
    public Material Skybox_Evening;
    public Material Skybox_Night;

    // enum for time of day
    public enum TOD
    {
        Morning,
        Evening,
        Night
    }

    public TOD currentTime = TOD.Morning;
    public float TransitionDuration = 5f;

    void Start()
    {
        UpdateLighting();
    }

    // switch time after pressing L key for testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            // cycle between Morning -> Evening -> Night
            if (currentTime == TOD.Morning)
                StartCoroutine(TransitionTo(TOD.Evening, TransitionDuration));
            else if (currentTime == TOD.Evening)
                StartCoroutine(TransitionTo(TOD.Night, TransitionDuration));
            else
                StartCoroutine(TransitionTo(TOD.Morning, TransitionDuration));
        }
    }

    public void SetTOD(TOD newTime)
    {
        currentTime = newTime;
        UpdateLighting();
    }

    void UpdateLighting()
    {
        switch (currentTime)
        {
            case TOD.Morning:
                RenderSettings.skybox = Skybox_Morning;
                directionalLight.color = new Color(0.8f, 0.9f, 1f); // warm morning light
                directionalLight.intensity = 0.7f;
                directionalLight.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
                RenderSettings.ambientLight = new Color(0.7f, 0.8f, 1f);
                break;

            case TOD.Evening:
                RenderSettings.skybox = Skybox_Evening;
                directionalLight.color = new Color(1f, 0.5f, 0.3f); // orange color
                directionalLight.intensity = 0.3f;
                directionalLight.transform.rotation = Quaternion.Euler(30f, 50f, 0f);
                RenderSettings.ambientLight = new Color(1f, 0.7f, 0.4f);
                break;

            case TOD.Night:
                RenderSettings.skybox = Skybox_Night;
                directionalLight.color = new Color(0.3f, 0.4f, 0.6f); // bluish color
                directionalLight.intensity = 0.1f;
                directionalLight.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
                RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.2f);
                break;
        }

        // update global illumination
        DynamicGI.UpdateEnvironment();
    }

    // transition between TOD
    IEnumerator TransitionTo(TOD newTime, float duration)
    {
        // starting values
        Color startColor = directionalLight.color;
        float startIntensity = directionalLight.intensity;
        Color startAmbient = RenderSettings.ambientLight;
        Material startSkybox = RenderSettings.skybox;

        // target values
        Color targetColor = new Color();
        float targetIntensity = 0f;
        Color targetAmbient = new Color();
        Material targetSkybox = Skybox_Morning;

        switch (newTime)
        {
            case TOD.Morning:
                targetColor = new Color(1f, 0.95f, 0.8f);
                targetIntensity = 0.8f;
                targetAmbient = new Color(1f, 0.9f, 0.7f);
                targetSkybox = Skybox_Morning;
                break;

            case TOD.Evening:
                targetColor = new Color(1f, 0.5f, 0.3f);
                targetIntensity = 0.3f;
                targetAmbient = new Color(1f, 0.7f, 0.4f);
                targetSkybox = Skybox_Evening;
                break;

            case TOD.Night:
                targetColor = new Color(0.3f, 0.4f, 0.6f);
                targetIntensity = 0.1f;
                targetAmbient = new Color(0.1f, 0.1f, 0.2f);
                targetSkybox = Skybox_Night;
                break;
        }

        // smooth transition
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            directionalLight.color = Color.Lerp(startColor, targetColor, time / duration);
            directionalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, time / duration);
            RenderSettings.ambientLight = Color.Lerp(startAmbient, targetAmbient, time / duration);
            RenderSettings.skybox.Lerp(startSkybox, targetSkybox, time / duration);
            yield return null;
        }

        // apply final state
        SetTOD(newTime);
    }
}
