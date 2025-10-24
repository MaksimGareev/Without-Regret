using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, IInventory
{
    private readonly List<ItemData> itemsList = new List<ItemData>();

    private List<ItemData> keyItems = new();
    private List<ItemData> otherItems = new();
    private List<ItemData> nonInventoryItems = new();

    public List<ItemData> KeyItems => keyItems;
    public List<ItemData> OtherItems => otherItems;
    public List<ItemData> NonInventoryItems => nonInventoryItems;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        if (item.ItemType == ItemType.Backpack)
        {
            nonInventoryItems.Add(item);

            if (showDebugLogs)
            {
                Debug.Log($"Backpack collected.");
            } 
        }
        else
        {
            itemsList.Add(item);

            if (showDebugLogs)
            {
                Debug.Log($"Added {item.ItemName} to inventory.");
            }  
        
            if (item.ItemType == ItemType.KeyItem)
            {
                keyItems.Add(item);
            }
            else
            {
                otherItems.Add(item);
            }
        }
        
    }

    public bool RemoveItem(ItemData item)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Removed {item.ItemName} from inventory.");
        }

        if (item.ItemType == ItemType.KeyItem)
        {
            keyItems.Remove(item);
        }
        else
        {
            otherItems.Remove(item);
        }
        
        return itemsList.Remove(item);
    }

    public IReadOnlyList<ItemData> GetItems()
    {
        return itemsList.AsReadOnly();
    }

    public ItemData GetFirstThrowable()
    {
        foreach (var item in itemsList)
        {
            if (item.ItemType == ItemType.ThrowableItem)
            {
                return item;
            }
        }
        
        return null;
    }
}

