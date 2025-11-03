using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 3;
    [SerializeField] private Button[] buttons;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private InventoryTooltipUI tooltipUI;
    [SerializeField] private Button keyItemsTabButton;
    [SerializeField] private Button otherItemsTabButton;
    [SerializeField] private Color highlightColor = new Color(0.70f, 0.70f, 0.70f);
    public enum InventoryTab { KeyItems, OtherItems }
    private InventoryTab currentTab = InventoryTab.OtherItems;

    [Header("Input Settings")]
    [SerializeField] private string tabLeftButton;
    [SerializeField] private string tabRightButton;
    [SerializeField] private float moveThreshold = 0.5f;
    [SerializeField] private float moveCooldown = 0.25f;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private Button[,] slotButtons;
    private Image[,] slotIcons;
    private ItemData[,] slotItems;

    private int selectedRow = 0;
    private int selectedColumn = 0;
    private float moveTimer = 0f;
    
    private PlayerEquipItem playerEquipItem;
    private Inventory inventory;
    private (int row, int col)? hoveredSlot = null;

    private void Awake()
    {

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        playerEquipItem = player?.GetComponent<PlayerEquipItem>();
        inventory = player?.GetComponent<Inventory>();

        InitializeSlots();
        RefreshInventoryUI();
    }

    private void OnEnable()
    {
        RefreshInventoryUI();
    }

    private void Start()
    {
        keyItemsTabButton.onClick.AddListener(() => SwitchTabs(InventoryTab.KeyItems));
        otherItemsTabButton.onClick.AddListener(() => SwitchTabs(InventoryTab.OtherItems));

        RefreshInventoryUI();
    }

    // Update is called once per frame
    void Update()
    {
        HandleControllerInput();
    }

    private void InitializeSlots()
    {
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

                    int capturedRow = row;
                    int capturedCol = col;

                    slotButtons[row, col].onClick.AddListener(() => OnSlotClicked(capturedRow, capturedCol, index));

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
                Debug.LogWarning("Couldnt InitializeSlots!");
                return;
            }
        }
        
        if (inventory == null)
        {
            inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
            if (inventory == null)
            {
                Debug.LogWarning("InventoryUIController.RefreshInventoryUI: No inventory found!");
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
                    slotIcons[row, col].sprite = itemsList[index].InvIcon;
                    slotIcons[row, col].color = Color.white;
                }
                else
                {
                    slotItems[row, col] = null;
                    slotIcons[row, col].sprite = emptySlotSprite;
                    slotIcons[row, col].color = new Color(1, 1, 1, 0.2f);
                }

                index++;
            }
        }

        HighlightSelectedSlot();
    }

    private void HandleControllerInput()
    {
        if (Input.GetButtonDown(tabLeftButton) || Input.GetButtonDown(tabRightButton))
        {
            if (currentTab == InventoryTab.KeyItems)
            {
                SwitchTabs(InventoryTab.OtherItems);
            }
            else
            {
                SwitchTabs(InventoryTab.KeyItems);
            }
        }
        
        moveTimer -= Time.unscaledDeltaTime;

        float horizontalInput = Input.GetAxis("Xbox RightStick X");
        float verticalInput = Input.GetAxis("Xbox RightStick Y");

        bool moved = false;

        if (moveTimer <= 0f)
        {
            if (Mathf.Abs(verticalInput) > Mathf.Abs(horizontalInput))
            {
                if (verticalInput > moveThreshold)
                {
                    MoveSelection(1, 0);
                    moved = true;
                }
                else if (verticalInput < -moveThreshold)
                {
                    MoveSelection(-1, -0);
                    moved = true;
                }
            }

            else if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput))
            {
                if (horizontalInput > moveThreshold)
                {
                    MoveSelection(0, -1);
                    moved = true;
                }
                else if (horizontalInput < -moveThreshold)
                {
                    MoveSelection(0, 1);
                    moved = true;
                }
            }
        }

        if (moved)
        {
            moveTimer = moveCooldown;
        }

        if (Input.GetButtonDown("Xbox A Button"))
        {
            OnSlotClicked(selectedRow, selectedColumn, selectedRow * columns + selectedColumn);
        }
    }
    
    private void MoveSelection(int rowChange, int columnChange)
    {
        selectedRow = Mathf.Clamp(selectedRow + rowChange, 0, rows - 1);
        selectedColumn = Mathf.Clamp(selectedColumn + columnChange, 0, columns - 1);
        HighlightSelectedSlot();
    }

    private void OnSlotClicked(int row, int column, int index)
    {
        ItemData clickedItem = slotItems[row, column];

        if (showDebugLogs && clickedItem == null)
        {
            Debug.Log("Clicked an empty slot.");
            return;
        }
        
        if (playerEquipItem != null && clickedItem.ItemType != ItemType.KeyItem)
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
    }

    private void HighlightSelectedSlot()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Image slotImage = slotButtons[row, col]?.GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.color = (row == selectedRow && col == selectedColumn) ? highlightColor : Color.white;
                    if (tooltipUI != null)
                    {
                        ItemData selectedItem = slotItems[selectedRow, selectedColumn];
                        if (selectedItem != null)
                        {
                            tooltipUI.Show(selectedItem);
                        }
                        else
                        {
                            tooltipUI.Hide();
                        }
                    }
                }
            }
        }
    }

    public void OnSlotPointerEnter(int row, int col)
    {
        hoveredSlot = (row, col);
        HighlightSelectedSlot();
    }

    public void OnSlotPointerExit()
    {
        hoveredSlot = null;
        HighlightSelectedSlot();
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

        if (currentTab == InventoryTab.KeyItems)
        {
            keyItemsTabButton.image.color = Color.white;
            otherItemsTabButton.image.color = highlightColor;
        }
        else
        {
            keyItemsTabButton.image.color = highlightColor;
            otherItemsTabButton.image.color = Color.white;
        }
    }
}
