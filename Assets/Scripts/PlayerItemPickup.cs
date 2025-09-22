using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerItemPickup : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;

    private WorldItem itemInRange;

    // Update is called once per frame
    void Update()
    {
        if (itemInRange != null && Input.GetKeyDown(pickupKey))
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
        Destroy(worldItem.gameObject);
        itemInRange = null;
    }
}
