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
    [SerializeField] private bool shouldShowIcon = true;
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

        popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
    }

    private void Update()
    {
        if (shouldShowIcon && !popupInstance.activeSelf)
        {
            popupInstance.SetActive(true);
        }
        else if (!shouldShowIcon && popupInstance.activeSelf)
        {
            popupInstance.SetActive(false);
        }
    }
    public void OnPlayerInteraction(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        inventory.itemToCollect = this;
        hasBeenCollected = true;
        gameObject.SetActive(false);
    }
    
}
