using UnityEngine;

public class PlayerEquipItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject equipTransform;
    public ItemData currentEquippedItem { get; private set; }
    private GameObject equippedItemInstance;
    private ToggleInventoryUI toggleInventoryUI;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    public bool throwableEquipped { get; private set; } = false;
    public bool grabbableEquipped { get; private set; } = false;
    public bool EquippableItemEquipped { get; private set; } = false;

    private bool tutorialShown = false;

    public void Awake()
    {
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
    }

    public void EquipItem(ItemData itemToEquip)
    {
        // UnequipItem if clicked on empty slot
        if (itemToEquip == null)
        {
            UnequipItem();
            return;
        }

        // Do nothing if player has their hands full
        if (throwableEquipped || grabbableEquipped || EquippableItemEquipped || PlayerComponents.playerMovingObjects.IsOccupied())
        {
            return;
        }

        // UnequipItem if clicked on the same slot as currently equipped item
        if (itemToEquip == currentEquippedItem)
        {
            UnequipItem();
            return;
        }

        if (equippedItemInstance != null)
        {
            Destroy(equippedItemInstance);
        }

        // Equip new item
        if (itemToEquip.VisualPrefab != null)
        {
            equippedItemInstance = Instantiate(itemToEquip.VisualPrefab, equipTransform.transform);
            equippedItemInstance.transform.localScale = itemToEquip.equippedScaleTransform;
            equippedItemInstance.transform
                .SetPositionAndRotation(equipTransform.transform.TransformPoint(itemToEquip.equippedPositionOffset), 
                equipTransform.transform.rotation * Quaternion.Euler(itemToEquip.equippedRotationOffset));
            currentEquippedItem = itemToEquip;
        }
        else
        {
            equippedItemInstance = null;
        }

        throwableEquipped = itemToEquip != null && itemToEquip.ItemType == ItemType.ThrowableItem;
        // Show tutorial for first time interaction
        if (!tutorialShown && InteractionTutorialUI.Instance != null && itemToEquip.ItemType == ItemType.ThrowableItem)
        {
            tutorialShown = true;
            if (toggleInventoryUI != null)
            {
                toggleInventoryUI.ToggleInventory();
                InteractionTutorialUI.Instance.ShowTutorial(InteractType.Throwable,
                "Hold the Right trigger / Right mouse button to aim your equiped throwable object.  Follow the indicator to know where the throwable item will land.  Holding the charge longer will result in a more direct throw.."
                 );
                return;
            }
        }


        grabbableEquipped = itemToEquip != null && itemToEquip.ItemType == ItemType.GrabbableItem;
        EquippableItemEquipped = itemToEquip != null && itemToEquip.ItemType == ItemType.EquippableItem;

        if (showDebugLogs)
        {
            Debug.Log($"Equipped {itemToEquip.ItemName}. Type = {itemToEquip.ItemType}");
        }
    }
    
    public void UnequipItem()
    {
        if (equippedItemInstance != null)
        {
            Destroy(equippedItemInstance);
            equippedItemInstance = null;
        }

        currentEquippedItem = null;
        throwableEquipped = false;
        grabbableEquipped = false;
        EquippableItemEquipped = false;

        if (showDebugLogs)
        {
            Debug.Log("Unequipped current item.");
        }
    }

    public GameObject GetEquippedItemInstance()
    {
        return equippedItemInstance;
    }
}
