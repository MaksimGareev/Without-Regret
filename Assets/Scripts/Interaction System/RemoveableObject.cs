using System;
using UnityEngine;

public class RemoveableObject : MonoBehaviour, IInteractable
{
    [Tooltip("Item required to remove this object. If null, no item is required and the object can be removed immediately.")]
    [SerializeField] ItemData requiredItem;
    public float interactionPriority => 1;
    public InteractType interactType => InteractType.Remove;
    public event Action OnInteracted;
    [SerializeField] private CleanupLeavesObjective objective;

    private bool interactable = true;

    void Start()
    {
        objective = (CleanupLeavesObjective)FindFirstObjectByType(typeof(CleanupLeavesObjective));
    }
    void OnEnable()
    {
        interactable = true;
    }
    void OnDisable()
    {
        interactable = false;
    }    

    public void OnPlayerInteraction(GameObject player)
    {
        if (!interactable) return;

        if (requiredItem != null && player.TryGetComponent<Inventory>(out var items))
        {
            if (!items.KeyItems.Contains(requiredItem))
            {
                // Required item is not in inventory
                Debug.Log($"Player tried to remove {gameObject.name}, but is missing required item {requiredItem.ItemName}");
                return;
            }
        }

        // Object is removed; notify listeners and disable gameobject
        OnInteracted?.Invoke();
        objective.AddLeaves();
        gameObject.SetActive(false);
        
    }

    public bool CanInteract(GameObject player)
    {
        return interactable;
    }

    public void SetInteractable(bool state)
    {
        interactable = state;
    }
}
