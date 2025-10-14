using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerItemPickup : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;
    [SerializeField] private string pickupButton = "Xbox X Button";
    [SerializeField] private GameObject interactingScript;
    private InventoryUIController inventoryUI;

    private WorldItem itemInRange;

    private void Awake()
    {
        inventoryUI = interactingScript.GetComponent<InventoryUIController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (itemInRange != null && (Input.GetKeyDown(pickupKey) || Input.GetButtonDown(pickupButton)))
        {
            CollectItem(itemInRange);
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

    private void CollectItem(WorldItem worldItem)
    {
        inventory.AddItem(worldItem.ItemData);
        inventoryUI.RefreshInventoryUI();
        Destroy(worldItem.gameObject);
        itemInRange = null;
    }
}
