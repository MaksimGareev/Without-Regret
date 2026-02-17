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
    
    private bool isObjectiveActive = false;

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
    }

  // private void Update()
  // {
  //     if (ObjectiveManager.Instance == null || linkedObjective == null) return;

  //     //isObjectiveActive = ObjectiveManager.Instance.IsObjectiveCompleted(linkedObjective.objectiveID);
  // }

  private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            isObjectiveActive = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (sceneToLoad == null || (!isObjectiveActive && needsObjective)) return;

        if (other.CompareTag("Player"))
        {
            LoadScene();
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
                    ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
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

        SceneManager.LoadScene(sceneToLoad.GetSceneName());
    }
}
