using UnityEngine;

public class CollectableItem : MonoBehaviour, IInteractable
{
    public float interactionPriority => 2f;
    public InteractType interactType => InteractType.Collectable;

    public bool CanInteract(GameObject player) => true;
    public bool isCollectible = true;
    [HideInInspector] public bool hasBeenCollected = false;
    [SerializeField] private float icondDistance = 3f;

    private Transform player;

    public void Start()
    {
        // Player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (hasBeenCollected)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
    public void OnPlayerInteraction(GameObject player)
    {
        if (!isCollectible || hasBeenCollected) return;

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null) return;
        //inventory.itemToCollect = this;

        hasBeenCollected = true;
        gameObject.SetActive(false);

        ButtonIcons.Instance?.Clear();
    }

}
