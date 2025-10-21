using UnityEngine;

public class PlayerEquipItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject equipTransform;
    private ItemData currentEquippedItem;
    private GameObject equippedItemInstance;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    
    public bool throwableEquipped { get; private set; } = false;

    public void EquipItem(ItemData itemToEquip)
    {
        // UnequipItem if clicked on empty slot
        if (itemToEquip == null)
        {
            UnequipItem();
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
        if (itemToEquip.VisualPrefab != null && itemToEquip.ItemType != ItemType.KeyItem)
        {
            equippedItemInstance = Instantiate(itemToEquip.VisualPrefab, equipTransform.transform);
            equippedItemInstance.transform.localPosition = Vector3.zero;
            equippedItemInstance.transform.localRotation = Quaternion.identity;
            currentEquippedItem = itemToEquip;
        }
        else
        {
            equippedItemInstance = null;
        }

        // Check if the item is throwable
        throwableEquipped = itemToEquip != null && itemToEquip.ItemType == ItemType.ThrowableItem;

        if (showDebugLogs)
        {
            Debug.Log($"Equipped {itemToEquip.name}. Throwable = {throwableEquipped}");
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

        if (showDebugLogs)
        {
            Debug.Log("Unequipped current item.");
        }
    }
}
