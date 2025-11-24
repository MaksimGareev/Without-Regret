using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, ISaveable
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
    [SerializeField] private bool hasBackpack = false;
    private PlayerController playerController;
    private PlayerEquipItem playerEquipItem;
    private InventoryUIController inventoryUI;
    private ToggleInventoryUI toggleInventoryUI;
    private CameraMovement cameraMovement;
    public WorldItem itemToCollect;

    public static event System.Action<ItemData> OnItemAdded;


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerEquipItem = GetComponent<PlayerEquipItem>();
        inventoryUI = interactingScript.GetComponent<InventoryUIController>();
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
        cameraMovement = Camera.main.GetComponent<CameraMovement>();
        itemToCollect = null;
    }

    private void Start()
    {
        if (hasBackpack)
        {
            SetBackpackActive();
            toggleInventoryUI.hasBackpack = true;
        }
        else
        {
            backpack.SetActive(false);
        }
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

    public void SaveTo(SaveData data)
    {
        data.inventorySaveData.items = itemsList;
        data.inventorySaveData.keyItems = keyItems;
        data.inventorySaveData.otherItems = otherItems;
        data.inventorySaveData.hasBackpack = hasBackpack;
    }

    public void LoadFrom(SaveData data)
    {
        itemsList.Clear();
        itemsList.AddRange(data.inventorySaveData.items);

        keyItems.Clear();
        keyItems.AddRange(data.inventorySaveData.keyItems);

        otherItems.Clear();
        otherItems.AddRange(data.inventorySaveData.otherItems);

        hasBackpack = data.inventorySaveData.hasBackpack;

        if (hasBackpack)
        {
            SetBackpackActive();
            toggleInventoryUI.hasBackpack = true;
        }

        inventoryUI.RefreshInventoryUI();
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
            //cameraMovement.TriggerPickupCameraEffect(itemToCollect.transform);
            //StartCoroutine(WaitForCameraTransition());
            itemToCollect.hasBeenCollected = true;
            Destroy(itemToCollect.gameObject, 1f);
            itemToCollect = null;
        }
        else
        {
            itemToCollect.hasBeenCollected = true;
            Destroy(itemToCollect.gameObject);
            itemToCollect = null;
        }

        OnItemAdded?.Invoke(item);

        inventoryUI.RefreshInventoryUI();

        SaveManager.Instance.SaveGame();
    }

    IEnumerator WaitForCameraTransition()
    {
        playerController.SetFreezePosition(true);
        yield return new WaitForSeconds(2f);
        playerController.SetFreezePosition(false);
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

        inventoryUI.RefreshInventoryUI();

        SaveManager.Instance.SaveGame();
        
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

