using UnityEngine;

public class PlayerEquipItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject equipTransform;
    public ItemData currentEquippedItem { get; private set; }
    private GameObject equippedItemInstance;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    public bool throwableEquipped { get; private set; } = false;
    public bool grabbableEquipped { get; private set; } = false;

    public void EquipItem(ItemData itemToEquip)
    {
        // UnequipItem if clicked on empty slot
        if (itemToEquip == null)
        {
            UnequipItem();
            return;
        }

        // Do nothing if player has grabbable item currently equipped
        if (grabbableEquipped)
        {
            return;
        }

        // Do nothing if player has a moveableObject currently held
        if (PlayerComponents.playerMovingObjects.IsOccupied())
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

        // Check if the item is throwable
        throwableEquipped = itemToEquip != null && itemToEquip.ItemType == ItemType.ThrowableItem;
        grabbableEquipped = itemToEquip != null && itemToEquip.ItemType == ItemType.GrabbableItem;

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
