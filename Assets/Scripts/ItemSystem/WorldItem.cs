using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    public ItemData ItemData => itemData;
    public float interactionPriority => 0f;
    [HideInInspector] public bool hasBeenCollected = false;
    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

    public void Start()
    {
        if (hasBeenCollected)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (shouldShowIcon && popupInstance == null)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }
    }
    public void OnPlayerInteraction(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        inventory.itemToCollect = this;
        hasBeenCollected = true;
        gameObject.SetActive(false);
        DisablePopupIcon();
    }

    public void EnablePopupIcon()
    {
        if (popupInstance == null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
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
    }
    
}
