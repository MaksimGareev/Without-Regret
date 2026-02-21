using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Inventory : MonoBehaviour, ISaveable
{
    private readonly List<ItemData> itemsList = new List<ItemData>();

    [HideInInspector] public List<ItemData> keyItems = new();
    [HideInInspector] public List<ItemData> otherItems = new();

    [HideInInspector] public List<ItemData> KeyItems => keyItems;
    [HideInInspector] public List<ItemData> OtherItems => otherItems;
    
    //private GameObject interactingScript;
    private GameObject backpack;
    //private TextMeshProUGUI AddItemPopup;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool hasBackpack = false;
    private PlayerController playerController;
    private PlayerEquipItem playerEquipItem;
    //private InventoryUIController inventoryUI;
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

        // if (interactingScript == null)
        // {
        //     var foundObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        //     interactingScript = System.Array.Find(foundObjects, obj => obj.name == "InteractingScript");
        //     if (interactingScript == null)
        //     {
        //         Debug.LogError("InteractingScript GameObject not found in the scene. Please ensure a GameObject named 'InteractingScript' exists in the scene as a child of the Inventory UI in the MainCanvas.");
        //     }
        // }

        // inventoryUI = interactingScript.GetComponent<InventoryUIController>();

        if (backpack == null)
        {
            backpack = GameObject.Find("PlayerBackpack");
            if (backpack == null)
            {
                Debug.LogError("Backpack GameObject not found in the scene. Please ensure a GameObject named 'PlayerBackpack' exists in the scene as a child of the player.");
            }
        }

        // if (AddItemPopup == null)
        // {
        //     //var foundObjects = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        //     AddItemPopup = GameObject.Find("Inventory Add popup")?.GetComponent<TextMeshProUGUI>();
        //     if (AddItemPopup == null)
        //     {
        //         Debug.LogError("AddItemPopup TextMeshProUGUI not found in the scene. Please ensure a TextMeshProUGUI named 'Inventory Add popup' exists in the scene as a child of the PlayerUICanvas.");
        //     }
        // }



        if(GameManager.Instance.inventoryPopupText != null)
            GameManager.Instance.inventoryPopupText.gameObject.SetActive(false);
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

        GameManager.Instance.inventoryInteractingScript.RefreshInventoryUI();
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
                cameraMovement.TriggerPickupCameraEffect(itemToCollect.transform);
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
}

