using UnityEngine;

public class BackpackObjective : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;
    [SerializeField] private string BackpackItemID = "Backpack";

    private void OnEnable()
    {
        Inventory.OnItemAdded += CheckForBag;
    }

    private void OnDisable()
    {
        Inventory.OnItemAdded -= CheckForBag;
    }

    private void CheckForBag(ItemData item)
    {
        // Identify "rose" however your ItemData is structured
        if (item.name == BackpackItemID)
        {
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            Debug.Log("Rose objective progress increased!");
        }
    }
}
