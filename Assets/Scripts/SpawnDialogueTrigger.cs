using UnityEngine;

public class SpawnDialogueTrigger : MonoBehaviour
{
    public GameObject dialogueTrigger;
    
    public ObjectiveData linkedObjective;
    public bool needsObjective;
    private bool objectiveActive = false;

    private void OnTriggerEnter(Collider other)
    {
        CheckIfObjectiveActive();
        
        if (linkedObjective && needsObjective && !objectiveActive) return;
        
        if (other.CompareTag("Player"))
        {
            dialogueTrigger.SetActive(true);
        }
    }

    private void CheckIfObjectiveActive()
    {
        if (linkedObjective && needsObjective && ObjectiveManager.Instance)
        {
            objectiveActive = ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID);
        }
    }
}
