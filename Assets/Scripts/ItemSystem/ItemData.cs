using UnityEngine;

public enum ItemType
{
    KeyItem,
    ThrowableItem,
    GrabbableItem,
    Backpack,
    Collectable,
    MiscItem,
    EquippableItem
}

[CreateAssetMenu(menuName = "ScriptableObjects/Item", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    [Tooltip("Name of the item. This is used for display purposes and should be unique for each item.")]
    [SerializeField] private string itemName;
    [Tooltip("Type of the item. This is used to determine how the item can be interacted with and what actions can be performed on it.")]
    [SerializeField] private ItemType itemType;
    [Tooltip("Icon to show in inventory. This is used to visually represent the item in the player's inventory UI.")]
    [SerializeField] private Sprite invIcon;
    [Tooltip("Prefab to spawn in the world when the item is dropped or spawned. This should have any necessary components for the item to function in the world (e.g. Rigidbody, Collider, WorldItem script, etc.).")]
    [SerializeField] private GameObject worldPrefab;
    [Tooltip("Prefab to spawn when the item is equipped. This should be a prefab with ONLY the mesh/model of the item. No other scripts or components need to be on this prefab.")]
    [SerializeField] private GameObject visualPrefab;

    public Vector3 equippedScaleTransform = Vector3.one;
    public Vector3 equippedPositionOffset;
    public Vector3 equippedRotationOffset;
    
    [Tooltip("Description of the item. This is used for display purposes when the player examines the item in the inventory.")]
    [SerializeField] private string description;

    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public Sprite InvIcon => invIcon;
    public GameObject WorldPrefab => worldPrefab;
    public GameObject VisualPrefab => visualPrefab;
    public string Description => description;
}
