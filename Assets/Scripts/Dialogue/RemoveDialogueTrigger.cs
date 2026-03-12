using UnityEngine;

public class RemoveDialogueTrigger : MonoBehaviour
{
    public string linkedObjectiveID;
    private ObjectiveManager objectiveManager;
    public GameObject triggers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        objectiveManager = ObjectiveManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (objectiveManager != null && objectiveManager.IsObjectiveCompleted(linkedObjectiveID))
        {
            triggers.SetActive(false);
        }
    }
}
