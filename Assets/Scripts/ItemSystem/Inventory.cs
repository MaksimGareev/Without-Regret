using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, IInventory
{
    private readonly List<ItemData> itemsList = new List<ItemData>();

    public void AddItem(ItemData item)
    {
        if (item == null) return;
        itemsList.Add(item);
        Debug.Log($"Added {item.ItemName} to inventory.");
    }

    public bool RemoveItem(ItemData item)
    {
        Debug.Log($"Removed {item.ItemName} from inventory.");
        return itemsList.Remove(item);
    }

    public IReadOnlyList<ItemData> GetItems()
    {
        return itemsList.AsReadOnly();
    }
}

