using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToNewLevel : MonoBehaviour
{
    public string sceneToLoad;
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

        isObjectiveActive = ObjectiveManager.Instance.IsObjectiveCompleted(linkedObjective.objectiveID);
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
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
