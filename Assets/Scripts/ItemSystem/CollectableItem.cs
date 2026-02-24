using UnityEngine;
using System.Collections;

public class CollectableItem : MonoBehaviour, IInteractable
{
    public float interactionPriority => 2f;
    public InteractType interactType => InteractType.Collectable;

    public bool CanInteract(GameObject player) => true;
    public bool isCollectible = true;
    [HideInInspector] public bool hasBeenCollected = false;
    [SerializeField] private float icondDistance = 3f;

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

        collectCoroutine = StartCoroutine(collectAnimationDelay());


        ButtonIcons.Instance?.Clear();
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
