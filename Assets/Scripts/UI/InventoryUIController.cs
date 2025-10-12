using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 3;
    [SerializeField] private Button[] allButtons;

    [Header("Input Settings")]
    [SerializeField] private float moveThreshold = 0.5f;
    [SerializeField] private float moveCooldown = 0.25f;

    private Button[,] slotButtons;
    private int selectedRow = 0;
    private int selectedColumn = 0;
    private float moveTimer = 0f;
    private PlayerEquipItem playerEquipItem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (allButtons.Length != rows * columns)
        {
            Debug.LogWarning($"InventoryUIController: Expected {rows * columns} buttons, found {allButtons.Length}.");
        }

        slotButtons = new Button[rows, columns];
        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (index < allButtons.Length)
                {
                    slotButtons[row, col] = allButtons[index];
                    int currentRow = row;
                    int currentCol = col;
                    int slotIndex = index;

                    slotButtons[row, col].onClick.AddListener(() => OnSlotClicked(currentRow, currentCol, slotIndex));
                    index++;
                }
            }
        }
        
        playerEquipItem = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerEquipItem>();
        HighlightSelectedSlot();
    }

    // Update is called once per frame
    void Update()
    {
        HandleControllerInput();
    }

    private void HandleControllerInput()
    {
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
                    MoveSelection(0, 1);
                    moved = true;
                }
                else if (verticalInput < -moveThreshold)
                {
                    MoveSelection(0, -1);
                    moved = true;
                }
            }

            else if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput))
            {
                if (horizontalInput > moveThreshold)
                {
                    MoveSelection(-1, 0);
                    moved = true;
                }
                else if (horizontalInput < -moveThreshold)
                {
                    MoveSelection(1, 0);
                    moved = true;
                }
            }
        }

        if (moved)
        {
            moveTimer = moveCooldown;
        }

        if (Input.GetButtonDown("Submit"))
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
        Debug.Log($"Clicked slot ({row},{column}) with index {index}");

        if (playerEquipItem != null)
        {
            playerEquipItem.EquipItem(index);
        }
        else
        {
            Debug.LogWarning("No PlayerEquipItem found!");
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
                    slotImage.color = (row == selectedRow && col == selectedColumn) ? Color.yellow : Color.white;
                }
            }
        }
    }
}
