using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToNewLevel : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("The scene that this trigger will load when the player enters it. Drag and drop the scene asset from the project window into this field.")]
    public SceneReference sceneToLoad;

    [Header("Objective Settings")]
    [Tooltip("Objective that must be ACTIVE to allow the player to trigger the scene transition. If the player does not have the linked objective ACTIVE, they will not be able to trigger the scene transition. When the player enters this trigger, it will add progress to the linked objective.")]
    public ObjectiveData linkedObjective;

    [Tooltip("If false, the player will be able to trigger the scene transition without needing to have the linked objective active, and the Linked Objective will be ignored. If true, the player must have the linked objective ACTIVE in order to trigger the scene transition, and the Linked Objective will need to be assigned.")]
    public bool needsObjective = true;
    
    [Tooltip("If true, the trigger will add progress to the linked objective when the player enters the trigger. If false, entering the trigger will not add progress to the linked objective, but will still be locked based on the needs objective toggle.")]
    public bool addProgress = false;
    
    private bool isObjectiveActive = false;
    private bool canTrigger = true;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
    }

    private void Start()
    {
        if (ObjectiveManager.Instance != null && linkedObjective != null && !isObjectiveActive)
        {
            isObjectiveActive = ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID);
        }
        canTrigger = false;
        CheckIfPlayerSpawnedInTrigger();
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            isObjectiveActive = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (sceneToLoad == null || (!isObjectiveActive && needsObjective) || !canTrigger) return;

        if (other.CompareTag("Player"))
        {
            LoadScene();
        }
    }

    private void CheckIfPlayerSpawnedInTrigger()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, GetComponent<Collider>().bounds.extents, Quaternion.identity);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                canTrigger = false;
                return;
            }
        }

        canTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !canTrigger)
        {
            canTrigger = true;
        }
    }

  void LoadScene()
    {
        if (needsObjective)
        {
            var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
            foreach (var obj in activeObjectives)
            {
                if (obj.data == linkedObjective)
                {
                    if (addProgress)
                    {
                        ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                    }

                    StartCoroutine(WaitToLoadScene());
                    return;
                }
                else
                {
                    Debug.Log("You must complete all objectives before moving forward");
                }
            }
        }
        else
        {
            StartCoroutine(WaitToLoadScene());
        }
    }

    private IEnumerator WaitToLoadScene()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
        
        yield return new WaitForSeconds(0.1f);

        if (sceneToLoad == null || string.IsNullOrEmpty(sceneToLoad.GetSceneName()))
        {
            Debug.LogError("Scene to load is not set or has an invalid name.");
            yield break;
        }

        if (GameManager.Instance != null && GameManager.Instance.sceneLoadManager != null)
        {
            GameManager.Instance.sceneLoadManager.LoadScene(sceneToLoad.GetSceneName());
        }
        else
        {
            Debug.LogError("SceneLoadManager reference is missing in the GameManager. Loading scene directly without fade transition.");
            SceneManager.LoadScene(sceneToLoad.GetSceneName());
        }
    }
}
