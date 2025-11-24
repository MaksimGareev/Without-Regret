using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour, ISaveable
{
    public static ObjectiveManager Instance;
    [Header("Objectives")]
    [SerializeField] private List<ObjectiveData> allObjectives;
    private int currentObjectiveIndex = 0;
    private List<ObjectiveInstance> activeObjectives = new();
    private List<ObjectiveInstance> completedObjectives = new();

    [Header("Events")]
    public UnityEvent<ObjectiveInstance> OnObjectiveActivated = new();
    public UnityEvent<ObjectiveInstance> OnObjectiveCompleted = new();

    [Header("UI Reference")]
    [SerializeField] private GameObject objectiveUI;

    private Coroutine UIHideRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveTo(SaveData data)
    {
        data.objectiveSaveData.currentObjectiveIndex = currentObjectiveIndex;
        data.objectiveSaveData.completedObjectives = completedObjectives;
        data.objectiveSaveData.activeObjectives = activeObjectives;
    }

    public void LoadFrom(SaveData data)
    {
        currentObjectiveIndex = data.objectiveSaveData.currentObjectiveIndex;
        completedObjectives = data.objectiveSaveData.completedObjectives;
        activeObjectives = data.objectiveSaveData.activeObjectives;

        ActivateObjective(allObjectives[currentObjectiveIndex]);
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
            Debug.LogWarning($"Objective is already active or completed.");
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
        SaveManager.Instance.SaveGame();
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

        SaveManager.Instance.SaveGame();
    }

    public void AddProgress(string ObjectiveID, int amount)
    {
        var objective = activeObjectives.Find(o => o.data.objectiveID == ObjectiveID);

        if (objective == null)
        {
            return;
        }

        objective.AddProgress(amount);

        if (objective.isCompleted)
        {
            CompleteObjective(objective);
        }

        SaveManager.Instance.SaveGame();
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (objectiveUI != null)
        {
            objectiveUI.SetActive(false);
        }

        yield return new WaitForSeconds(1f);

        foreach (var next in allObjectives)
        {
            if (!completedObjectives.Exists(o => o.data == next))
            {
                ActivateObjective(next);
                yield break;
            }
        }

        Debug.Log("All objectives complete");

        UIHideRoutine = null;
    }

    private void CompleteObjective(ObjectiveInstance objective)
    {
        objective.data.isCompleted = true;
        completedObjectives.Add(objective);
        activeObjectives.Remove(objective);
        OnObjectiveCompleted.Invoke(objective);

        if (UIHideRoutine != null)
        {
            StopCoroutine(UIHideRoutine);
        }

        UIHideRoutine = StartCoroutine(HideAfterDelay());
        currentObjectiveIndex++;

        SaveManager.Instance.SaveGame();
    }

    // check if a specific objective is completed
    public bool IsObjectiveCompleted(string id)
    {
        return completedObjectives.Exists(o => o.data.objectiveID == id);
    }

    // check if the player has completed all of the objectives in the current scene
    public bool AllObjectivesCompletedInScene(string sceneName)
    {
        foreach (var objective in allObjectives)
        {
            if (objective.sceneName == sceneName && !objective.isCompleted)
            {
                return false;
            }
        }
        return true;
    }

    // check if a specific objective is active
    public bool IsObjectiveActive(string id)
    {
        return activeObjectives.Exists(o => o.data.objectiveID == id);
    }

    public IEnumerable<ObjectiveInstance> GetActiveObjectives() => activeObjectives;
    public IEnumerable<ObjectiveInstance> GetCompletedObjectives() => completedObjectives;
}
