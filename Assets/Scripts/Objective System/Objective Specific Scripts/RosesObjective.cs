using UnityEngine;

public class RosesObjective : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;
    [SerializeField] private ItemData linkedItem;
    private bool itemsEnabled = false;

    private void OnEnable()
    {
        Inventory.OnItemAdded += CheckForRose;
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(EnableRoses);
    }

    private void OnDisable()
    {
        Inventory.OnItemAdded -= CheckForRose;
    }

    private void Start()
    {
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
        {
            EnableRoses(new ObjectiveInstance(linkedObjective));
        }
    }

    private void EnableRoses(ObjectiveInstance objective)
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

    private void CheckForRose(ItemData item)
    {
        // Identify "rose" however your ItemData is structured
        if (item.ItemType == linkedItem.ItemType && item.ItemName == linkedItem.ItemName)
        {
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            Debug.Log("Rose objective progress increased!");
        }
        else
        {
            Debug.Log($"{gameObject.name}: Item added is not the linked item. Item Name: {item.ItemName}, Item Type: {item.ItemType}, Expected Name: {linkedItem.ItemName}, Expected Type: {linkedItem.ItemType}");
        }
    }
}
