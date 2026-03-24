using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Inventory system for the player. This script manages the player's inventory, including adding and removing items, as well as saving and loading the inventory state.
public class Inventory : MonoBehaviour, ISaveable
{
    private readonly List<ItemData> itemsList = new List<ItemData>(); // List of all items
    public List<ItemData> keyItems = new(); // List of key items to show on the Key items tab
    public List<ItemData> otherItems = new(); // List of other items to show on the Other items tab

    [HideInInspector] public List<ItemData> KeyItems => keyItems; // Public getter for key items list
    [HideInInspector] public List<ItemData> OtherItems => otherItems; // Public getter for other items list
    private GameObject backpack; // Reference to the physical Backpack GameObject on the player
    [SerializeField] private bool hasBackpack = false;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    private bool inventoryLoaded = false; // Flag to indicate if the inventory has finished loading from save data
    private PlayerController playerController;
    private PlayerEquipItem playerEquipItem;
    private ToggleInventoryUI toggleInventoryUI;
    private CameraMovement cameraMovement;
    [HideInInspector] public WorldItem itemToCollect;
    public static event System.Action<ItemData> OnItemAdded;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerEquipItem = GetComponent<PlayerEquipItem>();
        
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
        cameraMovement = Camera.main.GetComponent<CameraMovement>();
        itemToCollect = null;

        if (backpack == null)
        {
            backpack = GameObject.Find("PlayerBackpack");
            if (backpack == null)
            {
                Debug.LogError("Backpack GameObject not found in the scene. Please ensure a GameObject named 'PlayerBackpack' exists in the scene as a child of the player.");
            }
        }

        if(GameManager.Instance.inventoryPopupText != null)
        {
            GameManager.Instance.inventoryPopupText.gameObject.SetActive(false);
        }
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
        inventoryLoaded = false;

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

        GameManager.Instance.inventoryInteractingScript.RefreshInventoryUI();

        inventoryLoaded = true;
    }
    
    private void SetBackpackActive()
    {
        backpack.SetActive(true);
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        OnItemAdded?.Invoke(item);

        StartCoroutine(ItemAddedPopUp(item));

        if (playerController != null)
        {
            StartCoroutine(playerController.CollectAnimationDelay());
        }

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

        if ((item.ItemType == ItemType.KeyItem || item.ItemType == ItemType.Backpack) && itemToCollect != null)
        {
            string[] scenesWOPickupEffect = { "MainMenu", "Echo'sHouse", "Echo'sHouseAstral", "BarryAndDarry'sHouse" };

            if (!System.Array.Exists(scenesWOPickupEffect, scene => scene == SceneManager.GetActiveScene().name))
            {
                cameraMovement?.TriggerPickupCameraEffect(itemToCollect.transform);
                StartCoroutine(WaitForCameraTransition());
            }
            
            itemToCollect.gameObject.SetActive(false);
            itemToCollect.hasBeenCollected = true;
            itemToCollect = null;
        }
        else if (itemToCollect != null)
        {
            itemToCollect.gameObject.SetActive(false);
            itemToCollect.hasBeenCollected = true;
            itemToCollect = null;
        }        

        GameManager.Instance.inventoryInteractingScript.RefreshInventoryUI();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
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

        GameManager.Instance.inventoryInteractingScript.RefreshInventoryUI();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

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

    private IEnumerator ItemAddedPopUp(ItemData Item)
    {
        GameManager.Instance.inventoryPopupText.text = "Item Added to Inventory: " + Item.ItemName;
        GameManager.Instance.inventoryPopupText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        GameManager.Instance.inventoryPopupText.gameObject.SetActive(false);
    }

    private void SetHasBackpack(bool newHasBackpack)
    {
        hasBackpack = newHasBackpack;
        if (hasBackpack)
        {
            SetBackpackActive();
            toggleInventoryUI.hasBackpack = true;
        }
        else
        {
            backpack.SetActive(false);
            toggleInventoryUI.hasBackpack = false;
        }
    }

    public IEnumerator OverwriteInventory(List<ItemData> newInventory, bool newHasBackpack)
    {
        while (!inventoryLoaded)
        {
            //Debug.LogWarning("Inventory not loaded yet, waiting...");
            yield return null;
        }

        itemsList.Clear();

        if (newInventory != null)
        {
            itemsList.AddRange(newInventory);
        }

        keyItems.Clear();
        otherItems.Clear();

        foreach (var item in newInventory)
        {
            if (item.ItemType == ItemType.KeyItem)
            {
                keyItems.Add(item);
            }
            else
            {
                otherItems.Add(item);
            }
        }

        SetHasBackpack(newHasBackpack);

        Debug.Log($"Inventory overwritten with {newInventory.Count} items. Backpack status: {newHasBackpack}");
        yield break;
    }
}

