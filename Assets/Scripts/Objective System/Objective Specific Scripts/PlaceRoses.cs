using UnityEngine;

public class PlaceRoses : MonoBehaviour, IInteractable
{
    [SerializeField] private ObjectiveData roseObjective;
    [SerializeField] private GameObject rosesVisual;
    [SerializeField] private ItemData roseItem;
    private Inventory playerInventory;

    public float interactionPriority => 0f;
    public InteractType interactType => InteractType.Pickup;

    private void Start()
    {
        if (rosesVisual != null)
        {
            rosesVisual.SetActive(false);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>();
        }
    }

    public void OnPlayerInteraction(GameObject player)
    {
        bool objectiveActive = false;

        var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();

        foreach (var obj in activeObjectives)
        {
            if (obj.data == roseObjective)
            {
                objectiveActive = true;
                break;
            }
        }

        if (roseObjective != null && ObjectiveManager.Instance != null && objectiveActive && playerInventory != null && playerInventory.KeyItems.Contains(roseItem))
        {
            ObjectiveManager.Instance.AddProgress(roseObjective.objectiveID, 1);
            ActivateRosesVisual();
            playerInventory.RemoveItem(roseItem);
        }
    }

    private void ActivateRosesVisual()
    {
        if (rosesVisual != null)
        {
            rosesVisual.SetActive(true);
        }
    }
}
