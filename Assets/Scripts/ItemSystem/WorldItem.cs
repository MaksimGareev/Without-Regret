using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    public ItemData ItemData => itemData;
    public float interactionPriority => 0f;

    public void OnPlayerInteraction(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        inventory.itemToCollect = this;
    }
    
}
