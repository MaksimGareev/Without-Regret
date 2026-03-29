using UnityEngine;

// An enum to define different item types.
// This is used to determine how the item interacts with the inventory and other player abilities like throwing or equipping.
public enum ItemType
{
    KeyItem,            // Important items that are not able to be equipped to hand
    ThrowableItem,      // Items that can be thrown by the player
    GrabbableItem,      // Items that can be equipped to the players hand but not stored in the inventory
    Backpack,           // Should only be used for backpack. Does not add to UI since it is the "inventory" itself
    Collectable,        // Will be used for collectibles that also show up in the journal (lore notes)
    EquippableItem,     // Default item type
    MiscItem,
}

// ScriptableObject that holds data for items in the game. 
// This is used to define the properties of an item, such as its name, description, icon, and prefabs for the world and visual representation.
[CreateAssetMenu(fileName = "NewItemData", menuName = "Items/ItemData", order = 1)]
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

    // Public getters for the private fields
    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public Sprite InvIcon => invIcon;
    public GameObject WorldPrefab => worldPrefab;
    public GameObject VisualPrefab => visualPrefab;
    public string Description => description;
}
