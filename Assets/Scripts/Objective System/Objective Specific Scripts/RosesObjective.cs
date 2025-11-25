using UnityEngine;

public class RosesObjective : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;
    [SerializeField] private ItemData linkedItem;

    private void OnEnable()
    {
        Inventory.OnItemAdded += CheckForRose;
    }

    private void OnDisable()
    {
        Inventory.OnItemAdded -= CheckForRose;
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
