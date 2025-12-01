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
    [SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);
    public bool shouldShowIcon = true;
    private GameObject popupInstance;
    public bool isCollectible = true;

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
        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
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
        if (!isCollectible) return;
        
        Inventory inventory = player.GetComponent<Inventory>();
        inventory.itemToCollect = this;
        hasBeenCollected = true;
        gameObject.SetActive(false);
        DisablePopupIcon();
    }

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
    }
    
}
