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

    [Tooltip("Distance at which the interaction icon will show up for this item.")]
    [SerializeField] private float icondDistance = 3f;

    [Tooltip("Whether this item can be collected by the player. Setting this to false will make the item non-interactable and it will not show an interaction prompt.")]
    public bool isCollectible = true;

    //[SerializeField] private GameObject iconPrefab;
    //[SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);

    //public bool shouldShowIcon = true;
    //private GameObject popupInstance;

    [HideInInspector] public bool hasBeenCollected = false;
    public ItemData ItemData => itemData;
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

    public bool CanInteract(GameObject player)
    {
        if (!isCollectible || hasBeenCollected || player == null)
            return false;

        PlayerMantling mantling = player.GetComponent<PlayerMantling>();
        if (mantling != null && mantling.isMantling)
            return false;

        return true;
    }

    void Update()
    {
        /*
        if (hasBeenCollected || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= icondDistance && isCollectible)
        {
            ButtonIcons.Instance.Highlight(interactType);
        }
        else
        {
            ButtonIcons.Instance.Clear();
        }

        
        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }*/
    }
    public void OnPlayerInteraction(GameObject player)
    {
        if (!isCollectible || hasBeenCollected) return;
        
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null) return;
        inventory.itemToCollect = this;

        hasBeenCollected = true;
        gameObject.SetActive(false);

        ButtonIcons.Instance?.Clear();
    }

    /*
    public void EnablePopupIcon()
    {
        if (popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
            gameObject.GetComponent<WorldPopup>().worldOffset = iconOffset;
            shouldShowIcon = true;
        }
    }

    public void DisablePopupIcon()
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
            popupInstance = null;
            shouldShowIcon = false;
        }
    }*/
    
}
