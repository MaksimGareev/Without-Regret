using UnityEngine;

// Central system that tracks the closest interactable object to the player
// and ensures UI stays visable while the player is near/interacting
public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private InteractableProximity currentInteractable;

    void Awake()
    {
        Instance = this;
    }

    // Called by interactable object to register themselves as potential targets
    public void RegisterInteractable(InteractableProximity interactable)
    {
        // If there is no current interactable or another is closer to the player
        // Replace current target
        if (currentInteractable == null || interactable.DistanceToPlayer < currentInteractable.DistanceToPlayer) 
        {
            currentInteractable = interactable;
        }
    }

    // Finalize interaction selection and trigger UI behavior
    public void LateUpdate()
    {
        if (currentInteractable != null)
        {
            // Safety check if UIFadeController is missing
            if (UIFadeController.Instance == null)
            {
                Debug.LogWarning("UIFadeController missing!");
            }

            // Keep UI visable if player is near an interactable object
            UIFadeController.Instance?.ShowUI();
        }

        // Reset every frame so only the closest interactable each frame is used
        currentInteractable = null;
    }
}
