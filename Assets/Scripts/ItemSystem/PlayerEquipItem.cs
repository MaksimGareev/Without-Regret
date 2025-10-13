using UnityEngine;

public class PlayerEquipItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerThrowing playerThrowing;
    [SerializeField] private GameObject[] equippableItemPrefabs;
    [SerializeField] private GameObject equipGameObject;

    private int equippedItemIndex = -1;
    private GameObject equippedItemInstance;

    public bool throwableEquipped { get; private set; } = false;

    public void EquipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippableItemPrefabs.Length)
        {
            Debug.LogWarning($"Invalid equip index {slotIndex}");
            return;
        }

        // UnequipItem if clicked again in inventory.
        if (equippedItemIndex == slotIndex)
        {
            UnequipItem();
            return;
        }

        // Unequip any currently equipped item before equipping a new item.
        if (equippedItemInstance != null)
        {
            Destroy(equippedItemInstance);
        }

        // Equip new item
        GameObject newItem = Instantiate(equippableItemPrefabs[slotIndex], equipGameObject.transform);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
        equippedItemInstance = newItem;
        equippedItemIndex = slotIndex;

        // Check if item is throwable
        var itemData = newItem.GetComponent<ItemData>();
        throwableEquipped = (itemData != null && itemData.ItemType == ItemType.ThrowableItem);

        Debug.Log($"Equipping item from slot {slotIndex}, throwable = {throwableEquipped}");
    }
    
    public void UnequipItem()
    {
        if (equippedItemInstance != null)
        {
            Destroy(equippedItemInstance);
            equippedItemInstance = null;
        }

        equippedItemIndex = -1;
        throwableEquipped = false;

        Debug.Log("Unequipped current item.");
    }
}
