using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ObjectiveManager : MonoBehaviour, ISaveable
{
    public static ObjectiveManager Instance { get; private set; }
    
    [Header("Objectives")]
    [SerializeField] private List<ObjectiveData> allObjectives;
    private int currentObjectiveIndex = 0;
    [SerializeField] private List<ObjectiveInstance> activeObjectives = new();
    private List<ObjectiveInstance> completedObjectives = new();
    private bool objectivesInSceneCompleted = false;

    [Header("Events")]
    [HideInInspector] public UnityEvent<ObjectiveInstance> OnObjectiveActivated = new();
    [HideInInspector] public UnityEvent<ObjectiveInstance> OnObjectiveCompleted = new();

    [Header("UI Reference")]
    [SerializeField] private GameObject objectiveUI;

    private Coroutine UIHideRoutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Register self with SaveManager as a savable entity
        StartCoroutine(RegisterWhenReady());
    }

    private void Start()
    {
        if (!SaveManager.Instance.SaveExists() && SceneManager.GetActiveScene().name != "Echo'sHouse")
        {
            //ActivateObjective(allObjectives[0]);
            //currentObjectiveIndex = 0;
            return;
        }
    }

    private IEnumerator RegisterWhenReady()
    {
        while (SaveManager.Instance == null)
        {
            yield return null;
        }

        SaveManager.Instance.RegisterSaveable(this);
        Debug.Log("ObjectiveManager Registered with SaveManager");
    }

    public void SaveTo(SaveData data)
    {
        Debug.Log($"[ObjectiveManager.SaveTo] called, activeObjectives.Count = {activeObjectives.Count}, completedObjectives.count = {completedObjectives.Count}");

        data.objectiveSaveData.currentObjectiveIndex = currentObjectiveIndex;

        data.objectiveSaveData.objectives.Clear();

        foreach(var inst in activeObjectives)
        {
            Debug.Log($"[ObjectiveManager.SaveTo] saving ACTIVE {inst.data.objectiveID} progress {inst.currentProgress}");
            data.objectiveSaveData.objectives.Add(new ObjectiveRecord
            {
                objectiveID = inst.data.objectiveID,
                progress = inst.currentProgress,
                isCompleted = false
            });
        }

        foreach (var inst in completedObjectives)
        {
            Debug.Log($"[ObjectiveManager.SaveTo] saving COMPLETED {inst.data.objectiveID} progress {inst.currentProgress}");
            data.objectiveSaveData.objectives.Add(new ObjectiveRecord
            {
                objectiveID = inst.data.objectiveID,
                progress = inst.currentProgress,
                isCompleted = true
            });
        }
    }

    public void LoadFrom(SaveData data)
    {
        currentObjectiveIndex = data.objectiveSaveData.currentObjectiveIndex;
        
        activeObjectives.Clear();
        completedObjectives.Clear();

        foreach (var record in data.objectiveSaveData.objectives)
        {
            ObjectiveData objective = allObjectives.Find(o => o.objectiveID == record.objectiveID);

            if (objective == null)
            {
                Debug.LogWarning($"Saved objective '{record.objectiveID}' not found!");
                continue;
            }

            ObjectiveInstance inst = new ObjectiveInstance(objective);
            inst.SetProgress(record.progress);

            if (record.isCompleted)
            {
                completedObjectives.Add(inst);
            }
            else
            {
                activeObjectives.Add(inst);
            }
        }
    }

    public void ActivateObjective(ObjectiveData objective)
    {
        if (objective == null)
        {
            Debug.LogWarning($"Objective is null");
            return;
        }

        if (activeObjectives.Exists(o => o.data == objective) || completedObjectives.Exists(o => o.data == objective))
        {
            Debug.LogWarning($"Objective '{objective.title}' is already active or completed.");
            return;
        }

        if (objectiveUI != null)
        {
            objectiveUI.SetActive(true);
        }

        if (UIHideRoutine != null)
        {
            StopCoroutine(UIHideRoutine);
            UIHideRoutine = null;
        }

        ObjectiveInstance newObjective = new ObjectiveInstance(objective);
        activeObjectives.Add(newObjective);
        OnObjectiveActivated.Invoke(newObjective);
        Debug.Log($"Objective '{newObjective.data.title}' has been activated");
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    public void ActivateObjectiveByID(string objectiveID)
    {
        ObjectiveData found = allObjectives.Find(o => o.objectiveID == objectiveID);

        if (found != null)
        {
            ActivateObjective(found);
            Debug.Log($"Objective '{found.title}' has been activated");
        }
        else
        {
            Debug.LogWarning($"Objective with ID '{objectiveID}' not found in list.");
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    public void AddProgress(string ObjectiveID, int amount)
    {
        var objective = activeObjectives.Find(o => o.data.objectiveID == ObjectiveID);

        if (objective == null)
        {
            return;
        }

        objective.AddProgress(amount);
        Debug.Log($"Objective '{objective.data.title}' progress increased to {objective.currentProgress}/{objective.data.requiredProgress}");

        if (objective.isCompleted)
        {
            CompleteObjective(objective);
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    private IEnumerator HideAfterDelay()
    {
        if (objectiveUI != null)
        {
            yield return new WaitForSeconds(2f);
            objectiveUI.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var next in allObjectives)
        {
            if (!completedObjectives.Exists(o => o.data == next))
            {
                ActivateObjective(next);
                yield break;
            }
        }

        objectivesInSceneCompleted = true;
        Debug.Log("All objectives complete");

        UIHideRoutine = null;
    }

    public void EnsureActiveObjective()
    {
        if (activeObjectives.Count > 0)
        {
            foreach (var obj in activeObjectives)
            {
                if (obj.isCompleted)
                {
                    CompleteObjective(obj);
                }
            }
        }

        for (int i = 0; i < allObjectives.Count; i++)
        {
            if (!completedObjectives.Exists(o => o.data == allObjectives[i]))
            {
                ActivateObjective(allObjectives[i]);
                return;
            }
        }

        Debug.Log("All objectives completed");
    }

    private void CompleteObjective(ObjectiveInstance objective)
    {
        objective.isCompleted = true;
        completedObjectives.Add(objective);
        activeObjectives.Remove(objective);
        OnObjectiveCompleted.Invoke(objective);

        Debug.Log($"Objective '{objective.data.title}' completed!");

        if (UIHideRoutine != null)
        {
            StopCoroutine(UIHideRoutine);
        }

        UIHideRoutine = StartCoroutine(HideAfterDelay());
        currentObjectiveIndex++;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }

    // check if a specific objective is completed
    public bool IsObjectiveCompleted(string id)
    {
        return completedObjectives.Exists(o => o.data.objectiveID == id);
    }

    // check if the player has completed all of the objectives in the current scene
    public bool AllObjectivesCompletedInScene()
    {
        return objectivesInSceneCompleted;
    }

    // check if a specific objective is active
    public bool IsObjectiveActive(string id)
    {
        return activeObjectives.Exists(o => o.data.objectiveID == id);
    }

    private void OnDestroy()
    {
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.RemoveSaveable(this);
        }
    }

    public void ClearObjectivesOnDelete()
    {
        activeObjectives.Clear();
        completedObjectives.Clear();

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            EnsureActiveObjective();
        }
    }

  public IEnumerable<ObjectiveInstance> GetActiveObjectives() => activeObjectives;
    public IEnumerable<ObjectiveInstance> GetCompletedObjectives() => completedObjectives;
}
