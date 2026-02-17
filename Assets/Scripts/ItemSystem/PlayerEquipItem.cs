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
    public bool EquippableItemEquipped { get; private set; } = false;

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
