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

    public void OnPlayerInteraction(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        inventory.itemToCollect = this;
        hasBeenCollected = true;
        gameObject.SetActive(false);
    }
    
}
