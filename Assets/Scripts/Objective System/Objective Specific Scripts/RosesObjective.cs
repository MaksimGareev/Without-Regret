using UnityEngine;

public class RosesObjective : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;
    [SerializeField] private string roseItemID = "Roses";

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
        if (item.name == roseItemID)
        {
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            Debug.Log("Rose objective progress increased!");
        }
    }
}
