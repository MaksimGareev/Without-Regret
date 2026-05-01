using UnityEngine;

// Central system that tracks the closest interactable object to the player
// and ensures UI stays visable while the player is near/interacting
public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private InteractableProximity currentInteractable;

    void Awake()
    {
        // Handle singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[InteractionManager] Another instance already exists, destroying this one");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[InteractionManager] Awake() - Instance set");
        
        // Reset state on scene load to ensure clean state
        currentInteractable = null;
    }

    private void OnEnable()
    {
        Debug.Log("[InteractionManager] OnEnable() called");
        
        // Validate instance is still set (safety check for scene reloads)
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[InteractionManager] Instance was null, setting it now");
        }
        
        // Reset state when scene loads
        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
            Debug.Log("[InteractionManager] Subscribed to OnSceneLoaded event");
        }
        else
        {
            Debug.LogWarning("[InteractionManager] SceneLoadManager.Instance is NULL on OnEnable");
        }
    }

    private void OnSceneLoaded()
    {
        Debug.Log("[InteractionManager] OnSceneLoaded event fired - clearing currentInteractable");
        // Clear interactable state when transitioning scenes
        currentInteractable = null;
    }

    // Called by interactable object to register themselves as potential targets
    public void RegisterInteractable(InteractableProximity interactable)
    {
        Debug.Log($"[InteractionManager] RegisterInteractable called with {interactable.gameObject.name}, distance: {interactable.DistanceToPlayer}");
        
        // If there is no current interactable or another is closer to the player
        // Replace current target
        if (currentInteractable == null || interactable.DistanceToPlayer < currentInteractable.DistanceToPlayer) 
        {
            currentInteractable = interactable;
            Debug.Log($"[InteractionManager] Updated currentInteractable to {interactable.gameObject.name}");
        }
    }

    // Finalize interaction selection and trigger UI behavior
    public void LateUpdate()
    {
        if (currentInteractable != null)
        {
            Debug.Log($"[InteractionManager] LateUpdate - currentInteractable: {currentInteractable.gameObject.name}");
            
            // Safety check if UIFadeController is missing
            if (UIFadeController.Instance == null)
            {
                Debug.LogWarning("[InteractionManager] UIFadeController.Instance is NULL!");
            }
            else
            {
                Debug.Log("[InteractionManager] Calling ShowUI()");
                // Keep UI visable if player is near an interactable object
                UIFadeController.Instance.ShowUI();
            }
        }

        // Reset every frame so only the closest interactable each frame is used
        currentInteractable = null;
    }

    private void OnDisable()
    {
        if (SceneLoadManager.Instance) SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(OnSceneLoaded);
    }
}
