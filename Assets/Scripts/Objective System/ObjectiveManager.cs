using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    [Header("Objectives")]
    [SerializeField] private List<ObjectiveData> allObjectives;
    private List<ObjectiveInstance> activeObjectives = new();
    private List<ObjectiveData> completedObjectives = new();

    [Header("Events")]
    public UnityEvent<ObjectiveInstance> OnObjectiveActivated = new();
    public UnityEvent<ObjectiveInstance> OnObjectiveCompleted = new();

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

    public void ActivateObjective(ObjectiveData objective)
    {
        if (objective == null)
        {
            Debug.LogWarning($"Objective is null");
            return;
        }

        if (activeObjectives.Exists(o => o.data == objective) || completedObjectives.Contains(objective))
        {
            Debug.LogWarning($"Objective is already completed.");
            return;
        }

        ObjectiveInstance newObjective = new ObjectiveInstance(objective);
        activeObjectives.Add(newObjective);
        OnObjectiveActivated.Invoke(newObjective);
        Debug.Log($"Objective '{newObjective.data.title}' has been activated");
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
    }

    private void CompleteObjective(ObjectiveInstance objective)
    {
        completedObjectives.Add(objective.data);
        activeObjectives.Remove(objective);
        OnObjectiveCompleted.Invoke(objective);
    }

    public IEnumerable<ObjectiveInstance> GetActiveObjectives() => activeObjectives;
    public IEnumerable<ObjectiveData> GetCompletedObjectives() => completedObjectives;
}
