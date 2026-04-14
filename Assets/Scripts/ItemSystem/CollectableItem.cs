using UnityEngine;
using System.Collections;

public class CollectableItem : MonoBehaviour, IInteractable
{
    [Tooltip("The journal entry associated with this collectable item. If empty, no entry is added")]
    [SerializeField] CollectableEntries collectableEntry;

    public float interactionPriority => 2f;
    public InteractType interactType => InteractType.Collectable;

    public bool CanInteract(GameObject player) => true;
    public bool isCollectible = true;
    [HideInInspector] public bool hasBeenCollected = false;
    //[SerializeField] private float iconDistance = 3f;

    [Header("Player Animator")]
    public Animator animator;
    public float collectAnimation;
    Coroutine collectCoroutine;
    public PlayerController playerController;

    private Transform player;

    public void Start()
    {
        // Player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player.GetComponent<PlayerController>();
        animator = player.GetComponentInChildren<Animator>();

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

        // Add journal entry when collected (if provided)
        if (collectableEntry != null && Journal.Instance != null)
        {
            float morality = NewDialogueManager.Instance.playerMorality;

            string finalDescription = collectableEntry.GetDescriptionByMorality(morality);

            Journal.Instance.AddCollectibleEntry(collectableEntry.entryTitle, finalDescription);
        }

        collectCoroutine = StartCoroutine(collectAnimationDelay());

        ButtonIcons.Instance?.Clear();
    }

    public void Initialize(CollectableEntries data, int playerMorality)
    {
        collectableEntry = data;

        // Lock in the description at spawn time
        if (collectableEntry != null)
        {
            if (playerMorality > 5)
            {
                collectableEntry.description = collectableEntry.positiveDescription;
            }
            else if (playerMorality < -5)
            {
                collectableEntry.description = collectableEntry.negativeDescription;
            }
            else
            {
                collectableEntry.description = collectableEntry.neutralDescription;
            }
        }
    }

    IEnumerator collectAnimationDelay()
    {
        animator.SetBool("isCollecting", true);
        animator.SetTrigger("collect");
        playerController.DisableInput();
        yield return new WaitForSeconds(collectAnimation);
        animator.SetBool("isCollecting", false);
        playerController.EnableInput();
        gameObject.SetActive(false);
    }

}
