using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("General Settings")]
    [Tooltip("ItemData that should be associated with this prefab. This is used to determine the name, description, and other properties of the item when collected.")]
    [SerializeField] private ItemData itemData;

    [Tooltip("Priority of this item's interaction. Lower priority items will be interacted with first if multiple items are in range.")]
    public float interactionPriority => 5f;

    [Tooltip("Type of interaction this item will have. This is used to determine the interaction prompt and icon that will show up when the player is in range.")]
    public InteractType interactType => InteractType.Pickup;

    [Tooltip("Whether this item can be collected by the player. Setting this to false will make the item non-interactable and it will not show an interaction prompt.")]
    public bool isCollectible = true;

    [Header("Objective Settings")]
    public List<string> requiredObjectives = new List<string>();

    [Header("Player Animation")]
    public float animationDuration = 1.5f; // Duration of the collect animation in seconds
    [HideInInspector] public bool hasBeenCollected = false;
    public ItemData ItemData => itemData;

    public void Start()
    {
        if (hasBeenCollected)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public bool CanInteract(GameObject player)
    {
        if (!isCollectible || hasBeenCollected || player == null)
            return false;

        bool allCompleted = true;

        if (requiredObjectives != null)
        {
            // check if all required objectives are completed if needed
            foreach (string id in requiredObjectives)
            {
                // if all required objectives are not completed set bool to false
                if (!ObjectiveManager.Instance.IsObjectiveCompleted(id))
                {
                    allCompleted = false;
                    return false;
                }
            }
        }

        PlayerMantling mantling = player.GetComponent<PlayerMantling>();
        if (mantling != null && mantling.isMantling)
            return false;

        return true;
    }
    
    public void OnPlayerInteraction(GameObject player)
    {
        if (!isCollectible || hasBeenCollected) return;
        
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null) return;
        inventory.itemToCollect = this;

        hasBeenCollected = true;

        ButtonIcons.Instance?.Clear();
    }
}
