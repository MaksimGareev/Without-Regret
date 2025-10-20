using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    KeyItem,
    ThrowableItem
}

[CreateAssetMenu(menuName = "ScriptableObjects/Item", order = 1)]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;
    [SerializeField] private Sprite invIcon;
    [SerializeField] private GameObject worldPrefab;
    [SerializeField] private GameObject visualPrefab;
    
    [TextArea(2, 5)]
    [SerializeField] private string description;

    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public Sprite InvIcon => invIcon;
    public GameObject WorldPrefab => worldPrefab;
    public GameObject VisualPrefab => visualPrefab;
    public string Description => description;
}
