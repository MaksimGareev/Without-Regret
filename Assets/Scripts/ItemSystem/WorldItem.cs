using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("General Settings")]
    [Tooltip("ItemData that should be associated with this prefab. This is used to determine the name, description, and other properties of the item when collected.")]
    [SerializeField] private ItemData itemData;

    [Tooltip("Priority of this item's interaction. Lower priority items will be interacted with first if multiple items are in range.")]
    public float interactionPriority => 5f;

    [Tooltip("Type of interaction this item will have. This is used to determine the interaction prompt and icon that will show up when the player is in range.")]
    public InteractType interactType => InteractType.Pickup;

    [Tooltip("Whether this item can be collected by the player. Setting this to false will make the item non-interactable and it will not show an interaction prompt.")]
    public bool isCollectible = true;

    [Header("Player Animation")]
    public float animationDuration = 1.5f; // Duration of the collect animation in seconds
    private Coroutine collectCoroutine;
    private Animator animator;
    private Transform player;
    private PlayerController playerController;
    [HideInInspector] public bool hasBeenCollected = false;
    public ItemData ItemData => itemData;

    public void Start()
    {
        // Player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player.GetComponent<PlayerController>();
        if (player != null)
        {
            animator = player.GetComponentInChildren<Animator>();
        }

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

    public bool CanInteract(GameObject player)
    {
        if (!isCollectible || hasBeenCollected || player == null)
            return false;

        PlayerMantling mantling = player.GetComponent<PlayerMantling>();
        if (mantling != null && mantling.isMantling)
            return false;

        return true;
    }
    
    public void OnPlayerInteraction(GameObject player)
    {
        if (!isCollectible || hasBeenCollected) return;
        
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null) return;
        inventory.itemToCollect = this;

        hasBeenCollected = true;
        collectCoroutine = StartCoroutine(CollectAnimationDelay());

        ButtonIcons.Instance?.Clear();
    }

    IEnumerator CollectAnimationDelay()
    {
        if (!animator)
        {
            Debug.LogWarning("Animator not found on the player in the scene");
            yield break;
        }

        animator.SetBool("isCollecting", true);
        animator.SetTrigger("collect");
        // playerController.DisableInput();
        yield return new WaitForSeconds(animationDuration);
        animator.SetBool("isCollecting", false);
        // playerController.EnableInput();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (collectCoroutine != null)
        {
            StopCoroutine(collectCoroutine);
            collectCoroutine = null;
        }
    }
}
