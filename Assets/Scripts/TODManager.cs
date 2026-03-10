using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

[System.Serializable]
public struct ObjectiveTODPair<ObjectiveData, TOD>
{
    public ObjectiveData objectiveData;
    public TOD time;
}

// enum for time of day
[System.Serializable]
public enum TOD
{
    Morning,
    Evening,
    Night
}

public class TODManager : MonoBehaviour
{
    public TOD currentTime = TOD.Morning;
    public float blendDuration = 2f;

    // spot lights that should turn on only during night
    public Light[] nightSpotLights;

    //  particles that should play only during night
    public ParticleSystem[] nightFogParticles;

    // global volumes for each time of day
    public Volume morningVolume;
    public Volume eveningVolume;
    public Volume nightVolume;

    
    [SerializeField] private List<ObjectiveTODPair<ObjectiveData, TOD>> objectiveTODList;

    private Dictionary<ObjectiveData, TOD> objectiveTODRuntime = new Dictionary<ObjectiveData, TOD>();
    private Coroutine blendCoroutine;

    private void Awake()
    {
        foreach (var pair in objectiveTODList)
        {
            if (!objectiveTODRuntime.ContainsKey(pair.objectiveData))
            {
                objectiveTODRuntime.Add(pair.objectiveData, pair.time);
            }
            else
            {
                Debug.LogWarning($"Duplicate objective data {pair.objectiveData} in objectiveTODList");
            }
        }
    }

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(ListenToObjective);
    }

    void Start()
    {
        ApplyTODImmediate();

        foreach (var pair in objectiveTODRuntime)
        {
            if (ObjectiveManager.Instance.IsObjectiveActive(pair.Key.objectiveID))
            {
                SetTOD(pair.Value);
                break;
            }
        }
    }

    // switch time after pressing l key for testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            // cycle between morning -> evening -> night
            if (currentTime == TOD.Morning)
                SetTOD(TOD.Evening);
            else if (currentTime == TOD.Evening)
                SetTOD(TOD.Night);
            else
                SetTOD(TOD.Morning);
        }
    }

    private void ListenToObjective(ObjectiveInstance objective)
    {
        foreach (var pair in objectiveTODRuntime)
        {
            if (pair.Key.objectiveID == objective.data.objectiveID)
            {
                SetTOD(pair.Value);
                break;
            }
        }
    }

    public void SetTOD(TOD newTime)
    {
        currentTime = newTime;

        if (blendCoroutine != null)
            StopCoroutine(blendCoroutine);

        blendCoroutine = StartCoroutine(BlendTOD());
    }

    void ApplyTODImmediate()
    {
        SetVolumeWeightsInstant();

        // update spot lights for night-only lighting
        UpdateNightSpotLights();

        // update fog particles for night-only effect
        UpdateNightFog();
    }

    // sets the correct global volume weight for the current time of day
    void SetVolumeWeightsInstant()
    {
        if (morningVolume != null)
            morningVolume.weight = (currentTime == TOD.Morning) ? 1f : 0f;

        if (eveningVolume != null)
            eveningVolume.weight = (currentTime == TOD.Evening) ? 1f : 0f;

        if (nightVolume != null)
            nightVolume.weight = (currentTime == TOD.Night) ? 1f : 0f;
    }

    IEnumerator BlendTOD()
    {
        float startMorning = (morningVolume != null) ? morningVolume.weight : 0f;
        float startEvening = (eveningVolume != null) ? eveningVolume.weight : 0f;
        float startNight = (nightVolume != null) ? nightVolume.weight : 0f;

        float targetMorning = (currentTime == TOD.Morning) ? 1f : 0f;
        float targetEvening = (currentTime == TOD.Evening) ? 1f : 0f;
        float targetNight = (currentTime == TOD.Night) ? 1f : 0f;

        // update spot lights for night-only lighting
        UpdateNightSpotLights();

        // update fog particles for night-only effect
        UpdateNightFog();

        float time = 0f;

        while (time < blendDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / blendDuration);

            if (morningVolume != null)
                morningVolume.weight = Mathf.Lerp(startMorning, targetMorning, t);

            if (eveningVolume != null)
                eveningVolume.weight = Mathf.Lerp(startEvening, targetEvening, t);

            if (nightVolume != null)
                nightVolume.weight = Mathf.Lerp(startNight, targetNight, t);

            yield return null;
        }

        SetVolumeWeightsInstant();
        blendCoroutine = null;
    }

    // enable or disable spot lights based on time of day
    void UpdateNightSpotLights()
    {
        bool lightsOn = (currentTime == TOD.Night);

        foreach (var light in nightSpotLights)
        {
            if (light != null)
                light.enabled = lightsOn;
        }
    }

    // enable or disable fog particle systems based on time of day
    void UpdateNightFog()
    {
        bool fogOn = (currentTime == TOD.Night);

        foreach (var fog in nightFogParticles)
        {
            if (fog != null)
            {
                if (fogOn && !fog.isPlaying)
                    fog.Play();
                if (!fogOn && fog.isPlaying)
                    fog.Stop();
            }
        }
    }

    private void OnDisable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.RemoveListener(ListenToObjective);
    }
}