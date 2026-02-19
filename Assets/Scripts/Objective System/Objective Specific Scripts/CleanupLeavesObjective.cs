using UnityEngine;

public class CleanupLeavesObjective : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveInactive);
    }

    private void Start()
    {
        // If the objective is already active (e.g. player is reloading a save), make sure the leaves are interactable
        if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
        {
            SetObjectiveActive(new ObjectiveInstance(linkedObjective));
        }
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            foreach (GameObject leaf in GameObject.FindGameObjectsWithTag("Leaves"))
            {
                if (leaf.TryGetComponent<RemoveableObject>(out var interactable))
                {
                    interactable.SetInteractable(true);
                    interactable.OnInteracted += IncrementCount; // Subscribe to the interaction event
                }
            }
        }
    }

    private void SetObjectiveInactive(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            foreach (GameObject leaf in GameObject.FindGameObjectsWithTag("Leaves"))
            {
                if (leaf.TryGetComponent<RemoveableObject>(out var interactable))
                {
                    interactable.SetInteractable(false);
                    interactable.OnInteracted -= IncrementCount; // Unsubscribe from the interaction event
                }
            }
        }
    }

    void IncrementCount()
    {
        ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
    }
}
