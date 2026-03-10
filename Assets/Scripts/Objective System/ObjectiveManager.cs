using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// The Objective Manager is responsible for managing the player's objectives throughout the game. 
// It keeps track of active and completed objectives, and handles the activation of new objectives as the player progresses. 
// It also communicates with the Objective UI to display the current objectives to the player, and with the Save System to save and load objective progress.
public class ObjectiveManager : MonoBehaviour, ISaveable
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Objectives")]
    [Tooltip("List of all objectives in the game. Objectives will be automatically activated in the order they are listed here. Next objective will only be activated once the previous one is completed.")]
    [SerializeField] private List<ObjectiveData> allObjectives;
    
    [Tooltip("List of currently active objectives. You should not modify this directly, as objectives will be added and removed from this list automatically as they are activated and completed. This should only be used for debugging purposes and for other scripts to access.")]
    [SerializeField] private List<ObjectiveInstance> activeObjectives = new();

    [Tooltip("List of completed objectives. You should not modify this directly, as objectives will be added to this list automatically when they are completed. This should only be used for debugging purposes and for other scripts to access.")]
    [SerializeField] private List<ObjectiveInstance> completedObjectives = new();
    
    [Tooltip("Reference to the ObjectiveCanvas that exists as a child of this Objective Manager Prefab.")]
    [SerializeField] private ObjectiveCanvas objectiveCanvas;

    [Tooltip("UI indicator for offscreen objectives")]
    [SerializeField] private OffscreenObjectiveIndicator ScreenSpaceIndicator;

    [Tooltip("In-world Objective Indicator")]
    [SerializeField] private GameObject WorldSpaceIndicator;

    // Events for when an objective is activated, updated, or completed. 
    // These are used to communicate with the Objective UI and other listeners so that they can react accordingly
    [HideInInspector] public UnityEvent<ObjectiveInstance> OnObjectiveActivated = new();
    [HideInInspector] public UnityEvent<ObjectiveInstance> OnObjectiveProgressUpdated = new();
    [HideInInspector] public UnityEvent<ObjectiveInstance> OnObjectiveCompleted = new();

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
        if(ScreenSpaceIndicator != null && WorldSpaceIndicator != null)
        {
            ScreenSpaceIndicator.target = WorldSpaceIndicator.transform;
        }
        // Find objective canvas reference in children, log error if not found
        objectiveCanvas = GetComponentInChildren<ObjectiveCanvas>();
        if (objectiveCanvas == null)
        {
            Debug.LogError("ObjectiveManager's ObjectiveCanvas reference is null. ObjectiveCanvas must be a child of this gameobject");
        }

        // Register self with SaveManager as a savable entity
        StartCoroutine(RegisterWhenReady());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    // Unsubscribe from the sceneLoaded event when the SaveManager is disabled to prevent memory leaks and unintended behavior
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }


    // Wait until SaveManager instance is available before registering, since SaveManager is 
    // also a singleton and may not be initialized yet when ObjectiveManager's Awake is called.
    private IEnumerator RegisterWhenReady()
    {
        while (SaveManager.Instance == null)
        {
            yield return null;
        }

        SaveManager.Instance.RegisterSaveable(this);
        // Debug.Log("ObjectiveManager Registered with SaveManager");
    }

    // Saves all active and completed objectives progress and completion status to the given SaveData
    // This is called by the SaveManager when saving the game
    public void SaveTo(SaveData data)
    {
        // Debug.Log($"[ObjectiveManager.SaveTo] called, activeObjectives.Count = {activeObjectives.Count}, completedObjectives.count = {completedObjectives.Count}");

        data.objectiveSaveData.objectives.Clear();
        
        // Assign all relevant data from the objective instance to an ObjectiveRecord, which is a serializable class that can be saved by the Save System.
        // Looping through active objectives list
        foreach (var inst in activeObjectives)
        {
            // Debug.Log($"[ObjectiveManager.SaveTo] saving ACTIVE {inst.data.objectiveID} progress {inst.currentProgress}");
            data.objectiveSaveData.objectives.Add(new ObjectiveRecord
            {
                objectiveID = inst.data.objectiveID,
                objectiveName = inst.data.title,
                progress = inst.currentProgress,
                isCompleted = false
            });
        }

        // Looping through completed objectives list
        foreach (var inst in completedObjectives)
        {
            // Debug.Log($"[ObjectiveManager.SaveTo] saving COMPLETED {inst.data.objectiveID} progress {inst.currentProgress}");
            data.objectiveSaveData.objectives.Add(new ObjectiveRecord
            {
                objectiveID = inst.data.objectiveID,
                objectiveName = inst.data.title,
                progress = inst.currentProgress,
                isCompleted = true
            });
        }
    }

    // Loads all objectives progress & completion status from the given SaveData
    // This is called by the SaveManager when loading a game
    public void LoadFrom(SaveData data)
    {
        // Clear current objectives lists before loading from save data
        activeObjectives.Clear();
        completedObjectives.Clear();

        foreach (var record in data.objectiveSaveData.objectives)
        {
            // Check if the objective from the save data exists in the list of all objectives, if not, skip it and log a warning
            ObjectiveData objective = allObjectives.Find(o => o.objectiveID == record.objectiveID);

            if (objective == null)
            {
                Debug.LogWarning($"Saved objective '{record.objectiveID}' not found!");
                continue;
            }

            // Create a new instance of the objective from the save data, then add it to the appropriate list
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

    // Handles logic for activating an objective
    public void ActivateObjective(ObjectiveData objective)
    {
        // Early returns if objective is null or already active/completed
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

        // Create new instance of the objective and add it to the active objectives list
        ObjectiveInstance newObjective = new ObjectiveInstance(objective);
        activeObjectives.Add(newObjective);

        // Fire event so ObjectiveUI (or other listeners) can react
        OnObjectiveActivated.Invoke(newObjective);

        //ManageObjectiveIndicator(objective);

        // Save the game after activating a new objective
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
    }

    // Helper method to activate and objective by its ID.
    // Not currently used, may be used later for side quests
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
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
    }

    // Handles logic for adding progress to an objective.
    public void AddProgress(string ObjectiveID, int amount)
    {
        // Find the objective in the active objectives list, return early if its not found
        var objective = activeObjectives.Find(o => o.data.objectiveID == ObjectiveID);

        if (objective == null)
        {
            return;
        }

        objective.AddProgress(amount);

        //Debug.Log($"Objective '{objective.data.title}' progress increased to {objective.currentProgress}/{objective.data.requiredProgress}");

        // Check if the objective is completed after adding progress, and if so, complete the objective
        if (objective.isCompleted)
        {
            CompleteObjective(objective);
        }
        else
        {
            // Only invoke progress update if not completed, otherwise completion event will handle it
            OnObjectiveProgressUpdated.Invoke(objective);
        }

        // Save the game after updating progress
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
    }

    // This coroutine is used to delay the activation of the next objective until after the objective completion UI has finished 
    // displaying so that the player is not immediately hit with a new objective popup right after completing an objective.
    private IEnumerator ActivateNextObjectiveAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        // Make sure the objective UI is not visible before activating the next objective
        if (objectiveCanvas == null)
        {
            objectiveCanvas = (ObjectiveCanvas)FindFirstObjectByType(typeof(ObjectiveCanvas));
        }

        // Wait until objective popup is no longer visible before activating the next objective
        if (objectiveCanvas != null)
        {
            yield return new WaitUntil(() => !objectiveCanvas.IsVisible());
        }
        else
        {
            Debug.LogWarning("No object with ObjectiveCanvas exists in this scene");
        }

        // Activate the next objective in the list that is not already completed
        foreach (var next in allObjectives)
        {
            if (!completedObjectives.Exists(o => o.data == next))
            {
                ActivateObjective(next);
                yield break;
            }
        }
    }

    // Ensures that an objective is automatically activated when the game starts
    // This is primarily called by the SaveManager upon loading a scene
    public void EnsureActiveObjective()
    {
        // Move objective to completed list if not already moved there
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

        // Activate the first objective in the list that is not already completed
        for (int i = 0; i < allObjectives.Count; i++)
        {
            if (!completedObjectives.Exists(o => o.data == allObjectives[i]))
            {
                ActivateObjective(allObjectives[i]);
                return;
            }
        }
    }

    // Handles logic for completing an objective
    private void CompleteObjective(ObjectiveInstance objective)
    {
        // Mark objective as completed and move it from active to completed list
        objective.isCompleted = true;
        completedObjectives.Add(objective);
        activeObjectives.Remove(objective);

        // Notify listeners (ObjectiveUI will display completion)
        OnObjectiveCompleted.Invoke(objective);

        Debug.Log($"Objective '{objective.data.title}' completed!");

        // Trigger next objective after UI (listeners) finished
        StartCoroutine(ActivateNextObjectiveAfterDelay());

        // Save the game
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
    }

    // Returns true if the objective is completed, false otherwise.
    public bool IsObjectiveCompleted(string id)
    {
        return completedObjectives.Exists(o => o.data.objectiveID == id);
    }

    // Returns true if the objective is currently active, false if not.
    public bool IsObjectiveActive(string id)
    {
        return activeObjectives.Exists(o => o.data.objectiveID == id);
    }

    // Remove itself from the SaveManager's list for tracking saveable objects
    private void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RemoveSaveable(this);
        }
    }

    // This is called by the SaveManager when the player deletes a save file to also clear the objectives lists.
    public void ClearObjectivesOnDelete()
    {
        activeObjectives.Clear();
        completedObjectives.Clear();

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            EnsureActiveObjective();
        }
    }

    // This method is used for testing purposes to skip to a specific objective and mark all previous objectives as completed.
    public void SkipToObjective(ObjectiveData objective)
    {
        if (objective == null)
        {
            Debug.LogWarning($"Objective is null");
            return;
        }

        if (!allObjectives.Contains(objective))
        {
            Debug.LogWarning($"Objective '{objective.title}' not found in list of all objectives.");
            return;
        }

        // Reset all objectives to not completed and not active
        activeObjectives.Clear();
        completedObjectives.Clear();

        // Mark all objecitves up to the given objective as completed
        foreach (var obj in allObjectives)
        {
            if (obj == objective)
            {
                break;
            }

            if (!completedObjectives.Exists(o => o.data == obj))
            {
                ObjectiveInstance inst = new ObjectiveInstance(obj);
                inst.SetProgress(obj.requiredProgress);
                CompleteObjective(inst);
            }
        }

        ActivateObjective(objective);
    }

    // private void ManageObjectiveIndicator(ObjectiveData objective)
    // {
    //     if (WorldSpaceIndicator == null || ScreenSpaceIndicator == null)
    //     {
    //         Debug.LogWarning("Objective indicators not assigned in ObjectiveManager");
    //         return;
    //     }

    //     if (objective.markerTransform != null)
    //     {
    //         Debug.Log("Active Scene: " + SceneManager.GetActiveScene().buildIndex);
    //         if (int.Equals(SceneManager.GetActiveScene().buildIndex, objective.sceneIndex) || int.Equals(SceneManager.GetActiveScene().buildIndex, 0))
    //         {
    //             WorldSpaceIndicator.transform.position = objective.markerTransform;
    //             WorldSpaceIndicator.SetActive(true);
    //         }
    //         else
    //         {
    //             WorldSpaceIndicator.SetActive(false);
    //         }
    //     }
    //     else
    //     {
    //         WorldSpaceIndicator.SetActive(false);
    //     }
    // }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if(SceneManager.GetActiveScene().buildIndex != 0)
        {
            EnsureActiveObjective();
            if(WorldSpaceIndicator != null && WorldSpaceIndicator.GetComponent<ObjectiveMarker>() != null)
            {
                WorldSpaceIndicator.GetComponent<ObjectiveMarker>().WorldIndicator.GetComponent<ObjectiveSpriteBillboard>().FindCamera();
            }
        }
    }

    // Getters for active and completed objectives lists.
    public IEnumerable<ObjectiveInstance> GetActiveObjectives() => activeObjectives;
    public IEnumerable<ObjectiveInstance> GetCompletedObjectives() => completedObjectives;
}