using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private InteractableProximity currentInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    public void RegisterInteractable(InteractableProximity interactable)
    {
        if (currentInteractable == null || interactable.DistanceToPlayer < currentInteractable.DistanceToPlayer) 
        {
            currentInteractable = interactable;
        }
    }

    public void LateUpdate()
    {
        if (currentInteractable != null)
        {
            if (UIFadeController.Instance == null)
            {
                Debug.LogWarning("UIFadeController missing!");
            }

            UIFadeController.Instance?.ShowUI();
        }

        currentInteractable = null;
    }
}
