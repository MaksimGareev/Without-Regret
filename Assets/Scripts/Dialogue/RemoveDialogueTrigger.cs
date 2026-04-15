using UnityEngine;

public class RemoveDialogueTrigger : MonoBehaviour
{
    public string linkedObjectiveID;
    private ObjectiveManager objectiveManager;
    public GameObject trigger1;
    public GameObject trigger2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        objectiveManager = ObjectiveManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectiveManager != null && objectiveManager.IsObjectiveCompleted(linkedObjectiveID))
            {
                trigger1.SetActive(false);
                trigger2.SetActive(false);
            }
            Debug.Log("player has exited");
        }
    }
}
