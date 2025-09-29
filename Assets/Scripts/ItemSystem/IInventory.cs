using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    void AddItem(ItemData item);
    bool RemoveItem(ItemData item);
    IReadOnlyList<ItemData> GetItems();
}