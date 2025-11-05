using UnityEngine;
using System.Collections;

public class TODManager : MonoBehaviour
{
    // sun and skybox references
    public Light directionalLight;
    public Material Skybox_Evening;
    public Material Skybox_Night;

    // enum time of day
    public enum TOD
    {
        Evening,
        Night
    }

    public TOD currentTime = TOD.Evening;

    public float TransitionDuration = 5f;

    void Start()
    {
        UpdateLighting();
    }

    // switches time after pressing L key for testing, delete/modify later
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (currentTime == TOD.Evening)
            {
                StartCoroutine(TransitionTo(TOD.Night, TransitionDuration));
            }
            else
            {
                StartCoroutine(TransitionTo(TOD.Evening, TransitionDuration));
            }
        }
    }

    public void SetTOD(TOD newTime)
    {
        currentTime = newTime;
        UpdateLighting();
    }
// if needed update this lines of code
    void UpdateLighting()
    {
        switch (currentTime)
        {
            case TOD.Evening:
                RenderSettings.skybox = Skybox_Evening;
                directionalLight.color = new Color(1f, 0.5f, 0.3f); // orange color
                directionalLight.intensity = 0.3f;// I wonder what light intensity does hmmmmmmmmm
                directionalLight.transform.rotation = Quaternion.Euler(30f, 50f, 0f); //sun rotation don't touch
                RenderSettings.ambientLight = new Color(1f, 0.7f, 0.4f); //ambient light
                break;
            case TOD.Night:
                RenderSettings.skybox = Skybox_Night;
                directionalLight.color = new Color(0.3f, 0.4f, 0.6f); // bluish color
                directionalLight.intensity = 0.3f;// I wonder what light intensity does hmmmmmmmmm
                directionalLight.transform.rotation = Quaternion.Euler(60f, 0f, 0f);//sun rotation don't touch
                RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.2f); //ambient light
                break;
        }
        // update global illumination
        DynamicGI.UpdateEnvironment();
    }

    // transition between TOD 
    IEnumerator TransitionTo(TOD newTime, float duration)
    {
        // starting values
        Color StartColor = directionalLight.color;
        float StartIntensity = directionalLight.intensity;
        Color startAmbient = RenderSettings.ambientLight;
        Material startSkybox = RenderSettings.skybox;

        // target updated values
        Color targetColor = new Color();
        float targetIntensity = 0f;
        Color targetAmbient = new Color();
        Material targetSkybox = new Material(Skybox_Evening);

        switch (newTime)
        {
            case TOD.Evening:
                targetColor = new Color(1f, 0.5f, 0.3f);
                targetIntensity = 0.2f;
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

        // smooth transition over time
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            directionalLight.color = Color.Lerp(StartColor, targetColor, time / duration);
            directionalLight.intensity = Mathf.Lerp(StartIntensity, targetIntensity, time / duration);
            RenderSettings.ambientLight = Color.Lerp(startAmbient, targetAmbient, time / duration);

            RenderSettings.skybox.Lerp(startSkybox, targetSkybox, time / duration);
            yield return null;
        }

        // set the new time of day after transition completes
        SetTOD(newTime);
    }
}
