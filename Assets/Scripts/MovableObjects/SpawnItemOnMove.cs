using UnityEngine;

public class SpawnItemOnMove : MonoBehaviour
{
    [SerializeField] MoveableObject moveableObject;
    [SerializeField] GameObject itemPrefabToSpawn;
    [SerializeField] Vector3 positionOffset = Vector3.zero;
    [SerializeField] Quaternion rotationOffset = Quaternion.identity;

    private void Awake()
    {
        if (moveableObject == null)
        {
            TryGetComponent<MoveableObject>(out moveableObject);
            if (moveableObject == null)
            {
                Debug.LogError("MoveableObject reference is not set and could not be found on the same GameObject.", this);
                return;
            }
        }
        if (itemPrefabToSpawn == null)
        {
            Debug.LogError("Item Prefab to Spawn is not set.", this);
            return;
        }

        // Subscribe to the object's OnInteracted event
        moveableObject.OnInteracted += SpawnItem;
    }

    void SpawnItem()
    {
        if (itemPrefabToSpawn != null)
        {
            Instantiate(itemPrefabToSpawn, moveableObject.transform.position + positionOffset, rotationOffset);
        }

        // Unsubscribe after spawning the item once
        moveableObject.OnInteracted -= SpawnItem;
    }
}
