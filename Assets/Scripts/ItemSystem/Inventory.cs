using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private readonly List<ItemData> itemsList = new List<ItemData>();

    private List<ItemData> keyItems = new();
    private List<ItemData> otherItems = new();

    public List<ItemData> KeyItems => keyItems;
    public List<ItemData> OtherItems => otherItems;

    [Header("References")]
    [SerializeField] private GameObject interactingScript;
    [SerializeField] private GameObject backpack;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    private bool hasBackpack = false;
    private PlayerController playerController;
    private PlayerEquipItem playerEquipItem;
    private InventoryUIController inventoryUI;
    private ToggleInventoryUI toggleInventoryUI;
    public WorldItem itemToCollect;


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerEquipItem = GetComponent<PlayerEquipItem>();
        inventoryUI = interactingScript.GetComponent<InventoryUIController>();
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
        itemToCollect = null;
    }

    private void Update()
    {
        if (itemToCollect != null)
        {
            if (itemToCollect.ItemData.ItemType == ItemType.GrabbableItem)
            {
                playerEquipItem.EquipItem(itemToCollect.ItemData);
                Destroy(itemToCollect.gameObject);
                itemToCollect = null;
            }
            else if (hasBackpack)
            {
                AddItem(itemToCollect.ItemData);
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"Player does not have backpack");
                }

                if (itemToCollect.ItemData.ItemType == ItemType.Backpack)
                {
                    AddItem(itemToCollect.ItemData);
                    Invoke(nameof(SetBackpackActive), 1f);

                    toggleInventoryUI.hasBackpack = true;
                }
            }
        }
    }
    
    private void SetBackpackActive()
    {
        backpack.SetActive(true);
    }

    private void AddItem(ItemData item)
    {
        if (item == null) return;

        if (item.ItemType == ItemType.Backpack)
        {
            hasBackpack = true;

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

        if (item.ItemType == ItemType.KeyItem || item.ItemType == ItemType.Backpack)
        {
            playerController.TriggerPickupCameraEffect(itemToCollect.transform);
            Destroy(itemToCollect.gameObject, 1f);
            itemToCollect = null;
        }
        else
        {
            Destroy(itemToCollect.gameObject);
            itemToCollect = null;
        }

        inventoryUI.RefreshInventoryUI();
    }

    public void RemoveItem(ItemData item)
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
        
        itemsList.Remove(item);
        
        return;
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

