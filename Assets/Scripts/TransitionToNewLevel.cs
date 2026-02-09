using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToNewLevel : MonoBehaviour
{
    public SceneReference sceneToLoad;
    public ObjectiveData linkedObjective;
    public bool needsObjective = true;
    private bool isObjectiveActive = false;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null || linkedObjective == null) return;

        //isObjectiveActive = ObjectiveManager.Instance.IsObjectiveCompleted(linkedObjective.objectiveID);
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
