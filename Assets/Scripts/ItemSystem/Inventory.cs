using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private readonly List<ItemData> itemsList = new List<ItemData>();

    private List<ItemData> keyItems = new();
    private List<ItemData> otherItems = new();
    private List<ItemData> nonInventoryItems = new();

    public List<ItemData> KeyItems => keyItems;
    public List<ItemData> OtherItems => otherItems;
    public List<ItemData> NonInventoryItems => nonInventoryItems;

    [Header("References")]
    [SerializeField] private GameObject interactingScript;
    [SerializeField] private GameObject backpack;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    private bool hasBackpack = false;
    private InventoryUIController inventoryUI;
    private ToggleInventoryUI toggleInventoryUI;
    public WorldItem itemToCollect;


    private void Awake()
    {
        inventoryUI = interactingScript.GetComponent<InventoryUIController>();
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
        itemToCollect = null;
    }

    private void Update()
    {
        if (itemToCollect != null)
        {
            if (hasBackpack)
            {
                CollectItem(itemToCollect);
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"Player does not have backpack");
                }

                if (CheckForBackpack(itemToCollect))
                {
                    CollectItem(itemToCollect);
                    backpack.SetActive(true);
                    hasBackpack = true;
                    toggleInventoryUI.hasBackpack = true;
                }
            }
        }
    }

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
    
    private bool CheckForBackpack(WorldItem worldItem)
    {
        return worldItem.ItemData.ItemType == ItemType.Backpack;
    }

    private void CollectItem(WorldItem worldItem)
    {
        AddItem(worldItem.ItemData);
        inventoryUI.RefreshInventoryUI();
        Destroy(worldItem.gameObject);
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

