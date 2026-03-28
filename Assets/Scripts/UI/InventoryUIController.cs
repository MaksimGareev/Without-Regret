using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryUIController : MonoBehaviour
{
    private enum InventoryTab { KeyItems, OtherItems }
    
    [Header("Grid Settings")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 3;
    [SerializeField] private Color highlightColor = new Color(0.70f, 0.70f, 0.70f, 0.70f);
    
    [Header("References")]
    [SerializeField] private Button[] buttons;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private InventoryTooltipUI tooltipUI;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Sprite rightTabSprite;
    [SerializeField] private Sprite leftTabSprite;
    [SerializeField] private Button keyItemsTabButton;
    [SerializeField] private Button otherItemsTabButton;

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    
    private InputAction navigateAction;
    private InputAction tabLeftButton;
    private InputAction tabRightButton;
    
    private InputActionReference defaultUIMoveAction;
    private InputSystemUIInputModule uiInputModule;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private InventoryTab currentTab = InventoryTab.OtherItems;
    
    private Button[,] slotButtons;
    private Image[,] slotIcons;
    private ItemData[,] slotItems;

    private GameObject currentSelectedSlot;
    private Dictionary<GameObject, (int row, int col)> slotLookup = new Dictionary<GameObject, (int row, int col)>();
    
    private PlayerEquipItem playerEquipItem;
    private Inventory inventory;
    
    private bool slotsInitialized = false;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        playerEquipItem = player?.GetComponent<PlayerEquipItem>();
        inventory = player?.GetComponent<Inventory>();

        InitializeInputActions();
    }



    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        Setup();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        Setup();
    }

    private void Setup()
    {
        InitializeSlots();
        RefreshInventoryUI();
        EnableInventoryInput();
        EventSystem.current.SetSelectedGameObject(slotButtons[0, 0].gameObject);
        SwitchTabs(currentTab);
    }

    private void Start()
    {
        keyItemsTabButton.onClick.AddListener(() => SwitchTabs(InventoryTab.KeyItems));
        otherItemsTabButton.onClick.AddListener(() => SwitchTabs(InventoryTab.OtherItems));

        RefreshInventoryUI();
        SwitchTabs(currentTab);
    }

    // Update is called once per frame
    void Update()
    {
        //HandleControllerInput();
        
        GameObject selectedSlot = EventSystem.current.currentSelectedGameObject;
        
        if (selectedSlot == currentSelectedSlot) return;
            
        currentSelectedSlot = selectedSlot;
        OnSelectionChanged(selectedSlot);
        
        // Switch tabs on input action triggered
        if (tabLeftButton.triggered || tabRightButton.triggered)
        {
            SwitchTabs(currentTab == InventoryTab.KeyItems ? InventoryTab.OtherItems : InventoryTab.KeyItems);
        }
    }

    private void InitializeInputActions()
    {
        // Initialize input actions
        navigateAction = inputActions.FindAction("Inventory/Navigate", true);
        tabLeftButton = inputActions.FindAction("Inventory/TabLeft", true);
        tabRightButton = inputActions.FindAction("Inventory/TabRight", true);
        //confirmButton = inputActions.FindAction("Inventory/Confirm", true);
    }

    private void InitializeSlots()
    {
        if (slotsInitialized)
        {
            return;
        }

        slotsInitialized = true;

        if (buttons.Length != rows * columns && showDebugLogs)
        {
            Debug.LogWarning($"InventoryUIController: Expected {rows * columns} buttons, found {buttons.Length}.");
        }

        slotButtons = new Button[rows, columns];
        slotIcons = new Image[rows, columns];
        slotItems = new ItemData[rows, columns];

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (index < buttons.Length)
                {
                    slotButtons[row, col] = buttons[index];
                    slotIcons[row, col] = slotButtons[row, col].transform.Find("Icon").GetComponent<Image>();
                    
                    // Capture row and col for listeners
                    int capturedRow = row;
                    int capturedCol = col;
                    
                    // Add Listener to on click
                    slotButtons[row, col].onClick.AddListener(() => OnSlotClicked(capturedRow, capturedCol, index));
                    
                    // Set explicit navigation for each slot
                    Navigation nav = new  Navigation();
                    nav.mode = Navigation.Mode.Explicit;
                    
                    nav.selectOnUp = (row > 0) ? slotButtons[row - 1, col] : null;
                    nav.selectOnDown = (row < rows - 1) ? slotButtons[row + 1, col] : null;
                    nav.selectOnLeft = (col > 0) ? slotButtons[row, col - 1] : null;
                    nav.selectOnRight = (col < columns - 1) ? slotButtons[row, col + 1] : null;
                    
                    slotButtons[row, col].navigation = nav;
                    
                    // Fill into slot look up dictionary
                    slotLookup[slotButtons[row, col].gameObject] = (row, col);
                    
                    // Add listeners to pointer enter and exit for mouse hover
                    EventTrigger trigger = slotButtons[row, col].GetComponent<EventTrigger>();
                    if (trigger == null)
                    {
                        trigger = slotButtons[row, col].gameObject.AddComponent<EventTrigger>();
                    }

                    EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                    entryEnter.callback.AddListener((_) => OnSlotHoverEnter(capturedRow, capturedCol));
                    trigger.triggers.Add(entryEnter);

                    EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                    entryExit.callback.AddListener((_) => OnSlotHoverExit());
                    trigger.triggers.Add(entryExit);

                    index++;
                }
            }
        }
    }

    public void RefreshInventoryUI()
    {
        if (slotButtons == null)
        {
            InitializeSlots();
            if (slotButtons == null)
            {
                if (showDebugLogs) Debug.LogWarning("Couldn't InitializeSlots!");
                return;
            }
        }
        
        if (!inventory)
        {
            inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Inventory>();
            if (!inventory)
            {
                if (showDebugLogs) Debug.LogWarning("InventoryUIController.RefreshInventoryUI: No inventory found!");
                return;
            }
        }

        IReadOnlyList<ItemData> itemsList = (currentTab == InventoryTab.KeyItems) ? inventory.KeyItems : inventory.OtherItems;
        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (index < itemsList.Count)
                {
                    slotItems[row, col] = itemsList[index];
                    if (itemsList[index]?.InvIcon)
                    {
                        slotIcons[row, col].sprite = itemsList[index].InvIcon;
                    }
                    else
                    {
                        if (showDebugLogs) Debug.LogWarning($"Item {itemsList[index].ItemName} is missing an inventory icon!");
                        if (emptySlotSprite)
                        {
                            slotIcons[row, col].sprite = emptySlotSprite;
                        }
                    }
                    slotIcons[row, col].color = Color.white;
                }
                else
                {
                    slotItems[row, col] = null;

                    if (emptySlotSprite)
                    {
                        slotIcons[row, col].sprite = emptySlotSprite;
                        slotIcons[row, col].color = Color.white;
                    }
                    else
                    {
                        slotIcons[row, col].color = new Color(1, 1, 1, 0f);
                    }
                }

                index++;
            }
        }

        OnSelectionChanged(EventSystem.current.currentSelectedGameObject);
    }

    // private void HandleControllerInput()
    // {
    //     if (tabLeftButton.WasPressedThisFrame() || tabRightButton.WasPressedThisFrame())
    //     {
    //         if (currentTab == InventoryTab.KeyItems)
    //         {
    //             SwitchTabs(InventoryTab.OtherItems);
    //         }
    //         else
    //         {
    //             SwitchTabs(InventoryTab.KeyItems);
    //         }
    //     }
    //     
    //     moveTimer -= Time.unscaledDeltaTime;
    //     
    //     float horizontalInput = navigateAction.ReadValue<Vector2>().x;
    //     float verticalInput = navigateAction.ReadValue<Vector2>().y;
    //     
    //     bool moved = false;
    //     
    //     if (moveTimer <= 0f)
    //     {
    //         if (Mathf.Abs(verticalInput) > Mathf.Abs(horizontalInput))
    //         {
    //             if (verticalInput > moveThreshold)
    //             {
    //                 MoveSelection(-1, 0);
    //                 moved = true;
    //             }
    //             else if (verticalInput < -moveThreshold)
    //             {
    //                 MoveSelection(1, 0);
    //                 moved = true;
    //             }
    //         }
    //     
    //         else if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput))
    //         {
    //             if (horizontalInput > moveThreshold)
    //             {
    //                 MoveSelection(0, 1);
    //                 moved = true;
    //             }
    //             else if (horizontalInput < -moveThreshold)
    //             {
    //                 MoveSelection(0, -1);
    //                 moved = true;
    //             }
    //         }
    //     }
    //     
    //     if (moved)
    //     {
    //         moveTimer = moveCooldown;
    //     }
    //     
    //     if (confirmButton.WasPressedThisFrame())
    //     {
    //         OnSlotClicked(selectedRow, selectedColumn, selectedRow * columns + selectedColumn);
    //     }
    // }
    
    // private void MoveSelection(int rowChange, int columnChange)
    // {
    //     // selectedRow = Mathf.Clamp(selectedRow + rowChange, 0, rows - 1);
    //     // selectedColumn = Mathf.Clamp(selectedColumn + columnChange, 0, columns - 1);
    //     HighlightSelectedSlot();
    // }

    private void OnSlotClicked(int row, int column, int index)
    {
        if (playerEquipItem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerEquipItem = player?.GetComponent<PlayerEquipItem>();

            if (playerEquipItem == null)
            {
                if (showDebugLogs) Debug.LogWarning("PlayerEquipItem still missing, cannot equip.");
                return;
            }
        }
        
        ItemData clickedItem = slotItems[row, column];

        if (clickedItem == null)
        {
            if (showDebugLogs)
            {
                Debug.Log("Clicked an empty slot.");
            }
            return;
        }
        
        if (clickedItem.ItemType != ItemType.KeyItem)
        {
            playerEquipItem.EquipItem(clickedItem);

            if (showDebugLogs)
            {
                Debug.Log($"Equipping item {clickedItem.name} from slot ({row},{column}) with index {index}.");
            }
        }
        else if (showDebugLogs)
        {
            if (clickedItem.ItemType == ItemType.KeyItem)
            {
                Debug.Log($"Unable to equip item type : {clickedItem.ItemType}");
            }
            else if (playerEquipItem == null)
            {
                Debug.LogWarning("PlayerEquipItem script not found on player!");
            }
            
        }
        
        OnSelectionChanged(slotButtons[row, column].gameObject);
    }

    private void OnSelectionChanged(GameObject selectedSlot)
    {
        if (!selectedSlot || !slotLookup.ContainsKey(selectedSlot)) return;
        
        var (selectedRow, selectedCol) = slotLookup[selectedSlot];
        
        // Highlight
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Image slotImage = slotButtons[row, col]?.GetComponent<Image>();
                
                if (slotImage)
                {
                    slotImage.color = (row == selectedRow && col == selectedCol)? highlightColor : Color.white;
                }
            }
        }
        
        // Tooltip
        ItemData selectedItem = slotItems[selectedRow, selectedCol];
        if (selectedItem)
        {
            tooltipUI?.Show(selectedItem);
        }
        else
        {
            tooltipUI?.Hide();
        }
    }

    public void OnSlotPointerEnter(int row, int col)
    {
        // hoveredSlot = (row, col);
        // HighlightSelectedSlot();
        
        currentSelectedSlot = slotButtons[row, col].gameObject;
        EventSystem.current.SetSelectedGameObject(currentSelectedSlot);
        OnSelectionChanged(currentSelectedSlot);
    }

    public void OnSlotPointerExit()
    {
        // hoveredSlot = null;
        // HighlightSelectedSlot();
        
        currentSelectedSlot = null;
        EventSystem.current.SetSelectedGameObject(null);
        OnSelectionChanged(null);
    }

    private void OnSlotHoverEnter(int row, int column)
    {
        if (tooltipUI == null || slotItems == null)
        {
            return;
        }

        ItemData item = slotItems[row, column];
        if (item != null)
        {
            tooltipUI.Show(item);
            if (showDebugLogs)
            {
                Debug.Log($"Hovering over item: {item.ItemName}");
            }
        }
    }

    private void OnSlotHoverExit()
    {
        if (tooltipUI == null)
        {
            return;
        }

        tooltipUI.Hide();
    }
    
    private void SwitchTabs(InventoryTab newTab)
    {
        currentTab = newTab;
        RefreshInventoryUI();

        if (backgroundPanel != null && rightTabSprite != null && leftTabSprite != null)
        {
            if (currentTab == InventoryTab.KeyItems)
            {
                backgroundPanel.sprite = leftTabSprite;

                // keyItemsTabButton.GetComponent<SelectableHighlighting>().stayHighlighted = true;
                // keyItemsTabButton.GetComponent<SelectableHighlighting>()?.ApplyHighlight(true);
                // otherItemsTabButton.GetComponent<SelectableHighlighting>()?.RemoveHighlight(true);
            }
            else
            {
                backgroundPanel.sprite = rightTabSprite;

                // otherItemsTabButton.GetComponent<SelectableHighlighting>().stayHighlighted = true;
                // otherItemsTabButton.GetComponent<SelectableHighlighting>()?.ApplyHighlight(true);
                // keyItemsTabButton.GetComponent<SelectableHighlighting>()?.RemoveHighlight(true);
            }
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        DisableInventoryInput();
    }

    private void EnableInventoryInput()
    {
        inputActions.FindActionMap("Inventory").Enable();
        inputActions.FindAction("Player/Look").Disable();
        inputActions.FindAction("Player/Jump").Disable();

        if (!uiInputModule)
        {
            uiInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (!uiInputModule)
            {
                Debug.LogWarning("UIInputModule not found on UIInputModule!");
            }
        }
        
        if (!defaultUIMoveAction)
        {
            defaultUIMoveAction = ScriptableObject.CreateInstance<InputActionReference>();
            if (!defaultUIMoveAction)
            {
                Debug.LogWarning("Failed to create defaultUIMoveAction reference!");
            }
        }
        
        defaultUIMoveAction = uiInputModule.move;
        uiInputModule.move = InputActionReference.Create(navigateAction);
    }

    private void DisableInventoryInput()
    {
        inputActions.FindActionMap("Inventory").Disable();
        inputActions.FindAction("Player/Look").Enable();
        inputActions.FindAction("Player/Jump").Enable();

        if (defaultUIMoveAction && uiInputModule)
        {
            uiInputModule.move = defaultUIMoveAction;
        }
    }
}
