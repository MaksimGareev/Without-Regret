using UnityEngine;

public class BackpackObjective : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;
    [SerializeField] private ItemData linkedItem;
    private bool itemsEnabled = false;

    private void Enable()
    {
        Inventory.OnItemAdded += CheckForBag;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(EnableBackpacks);
            Debug.Log($"{gameObject.name}: Subscribed to OnObjectiveActivated event.");
        }
        else
        {
            Debug.LogWarning("ObjectiveManager instance is null. Cannot subscribe to OnObjectiveActivated event.");
        }
        
    }

    private void OnDisable()
    {
        Inventory.OnItemAdded -= CheckForBag;
    }

    private void EnableBackpacks(ObjectiveInstance objective)
    {
        Debug.Log($"{gameObject.name}: EnableBackpacks called for objective: {objective.data.objectiveID}");

        if (itemsEnabled || objective.data != linkedObjective) 
        {
            Debug.Log($"{gameObject.name}: Items already enabled or objective does not match. itemsEnabled: {itemsEnabled}, current objective: {objective.data.objectiveID}, linked objective: {linkedObjective.objectiveID}");
            return;
        }

        WorldItem[] items = FindObjectsByType<WorldItem>(FindObjectsSortMode.None);

        if (items.Length == 0)
        {
            Debug.LogWarning("No WorldItem instances found in the scene.");
            return;
        }

        foreach (WorldItem item in items)
        {
            if (item.ItemData.ItemType == linkedItem.ItemType && item.ItemData.ItemName == linkedItem.ItemName)
            {
                Debug.Log($"Enabling backpack item: {item.gameObject.name}");
                item.isCollectible = true;
            }
            else 
            {
                Debug.Log($"Not enabling item: {item.gameObject.name} - does not match linked item.");
            }
        }

        itemsEnabled = true;
    }

    private void CheckForBag(ItemData item)
    {
        // Identify "backpack" however your ItemData is structured
        if (item.ItemType == linkedItem.ItemType && item.ItemName == linkedItem.ItemName)
        {
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            Debug.Log("Backpack objective progress increased!");
        }
        else
        {
            Debug.Log($"{gameObject.name}: Item added is not the linked item. Item Name: {item.ItemName}, Item Type: {item.ItemType}, Expected Name: {linkedItem.ItemName}, Expected Type: {linkedItem.ItemType}");
        }
    }
}
