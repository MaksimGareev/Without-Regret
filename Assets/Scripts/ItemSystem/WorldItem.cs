using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    public ItemData ItemData => itemData;
    public float interactionPriority => 0f;
    public InteractType interactType => InteractType.Pickup;

    [HideInInspector] public bool hasBeenCollected = false;

    //[SerializeField] private GameObject iconPrefab;
    //[SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);

    //public bool shouldShowIcon = true;
    //private GameObject popupInstance;

    [SerializeField] private float icondDistance = 3f;

    public bool isCollectible = true;

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
