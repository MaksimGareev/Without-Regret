using UnityEngine;

// This script is attached to trigger volumes in the scene that are linked to specific objectives. 
// When the player enters the trigger volume, it will add progress to the linked objective in the Objective Manager.
public class ObjectiveTriggerVolume : MonoBehaviour
{
    [Header("Objective Trigger Settings")]
    [Tooltip("The ObjectiveData asset that this trigger volume is linked to. When the player enters this trigger, it will add progress to the linked objective.")]
    [SerializeField] private ObjectiveData linkedObjective;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger volume, and if there is a valid linked objective
        if (other.CompareTag("Player") && linkedObjective != null && ObjectiveManager.Instance != null)
        {
            var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
            
            // If the linked objective is currently active, add progress to it
            foreach (var obj in activeObjectives)
            {
                if (obj.data == linkedObjective)
                {
                    ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                    break;
                }
            }
        }
    }
}
