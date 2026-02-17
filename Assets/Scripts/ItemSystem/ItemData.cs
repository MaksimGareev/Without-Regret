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
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;
    [SerializeField, Tooltip("Icon that appears in the Inventory")] private Sprite invIcon;
    [SerializeField, Tooltip("Object that should be placed in the world")] private GameObject worldPrefab;
    [SerializeField, Tooltip("Object that appears in Echo's hands")] private GameObject visualPrefab;

    public Vector3 equippedScaleTransform = Vector3.one;
    public Vector3 equippedPositionOffset;
    public Vector3 equippedRotationOffset;

    [SerializeField] private string description;

    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public Sprite InvIcon => invIcon;
    public GameObject WorldPrefab => worldPrefab;
    public GameObject VisualPrefab => visualPrefab;
    public string Description => description;
}
