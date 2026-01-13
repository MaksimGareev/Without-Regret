using UnityEngine;

public class BackpackObjective : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;
    [SerializeField] private ItemData linkedItem;
    private bool itemsEnabled = false;

    private void OnEnable()
    {
        Inventory.OnItemAdded += CheckForBag;
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(EnableBackpacks);
    }

    private void OnDisable()
    {
        Inventory.OnItemAdded -= CheckForBag;
    }

    private void EnableBackpacks(ObjectiveInstance objective)
    {
        if (itemsEnabled || objective.data != linkedObjective) return;

        WorldItem[] items = FindObjectsByType<WorldItem>(FindObjectsSortMode.None);

        foreach (WorldItem item in items)
        {
            if (item.ItemData.ItemType == linkedItem.ItemType && item.ItemData.ItemName == linkedItem.ItemName)
            {
                item.isCollectible = true;
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
