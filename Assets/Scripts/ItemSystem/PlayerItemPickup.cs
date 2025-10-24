using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerItemPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject interactingScript;
    [SerializeField] private GameObject backpack;

    [Header("Input Settings")]
    [SerializeField] private KeyCode pickupKey = KeyCode.F;
    [SerializeField] private string pickupButton = "Xbox X Button";

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    
    private InventoryUIController inventoryUI;
    private ToggleInventoryUI toggleInventoryUI;
    private bool hasBackpack = false;
    private WorldItem itemInRange;

    private void Awake()
    {
        inventoryUI = interactingScript.GetComponent<InventoryUIController>();
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (itemInRange != null && (Input.GetKeyDown(pickupKey) || Input.GetButtonDown(pickupButton)))
        {
            if (hasBackpack)
            {
                CollectItem(itemInRange);
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"Player does not have backpack");
                }

                if (CheckForBackpack(itemInRange))
                {
                    CollectItem(itemInRange);
                    backpack.SetActive(true);
                    hasBackpack = true;
                    toggleInventoryUI.hasBackpack = true;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WorldItem worldItem = other.GetComponent<WorldItem>();
        if (worldItem != null)
        {
            itemInRange = worldItem;
            // Show UI Prompt
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WorldItem worldItem = other.GetComponent<WorldItem>();
        if (worldItem != null && worldItem == itemInRange)
        {
            itemInRange = null;
            // Remove UI Prompt
        }
    }

    private bool CheckForBackpack(WorldItem worldItem)
    {
        return worldItem.ItemData.ItemType == ItemType.Backpack;
    }

    private void CollectItem(WorldItem worldItem)
    {
        inventory.AddItem(worldItem.ItemData);
        inventoryUI.RefreshInventoryUI();
        Destroy(worldItem.gameObject);
        itemInRange = null;
    }
}
