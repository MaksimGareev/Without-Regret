using UnityEngine;

[RequireComponent(typeof(RemoveableObject))]
public class SpawnItemOnRemoval : MonoBehaviour
{
    private RemoveableObject removeableObject;
    [SerializeField] GameObject itemPrefabToSpawn;
    [SerializeField] Vector3 positionOffset = Vector3.zero;
    [SerializeField] Quaternion rotationOffset = Quaternion.identity;

    private GameObject obj;

    private void Awake()
    {
        if (removeableObject == null)
        {
            TryGetComponent<RemoveableObject>(out removeableObject);
            if (removeableObject == null)
            {
                Debug.LogError("RemoveableObject reference is not set and could not be found on the same GameObject.", this);
                return;
            }
        }
        if (itemPrefabToSpawn == null)
        {
            Debug.LogError("Item Prefab to Spawn is not set.", this);
            return;
        }

        obj = Instantiate(itemPrefabToSpawn, removeableObject.transform.position + positionOffset, rotationOffset);
        obj.SetActive(false);

        // Subscribe to the object's OnInteracted event
        removeableObject.OnInteracted += SpawnItem;
    }

    void SpawnItem()
    {
        if (obj != null)
        {
            obj.SetActive(true);
        }

        // Unsubscribe after spawning the item once
        removeableObject.OnInteracted -= SpawnItem;
    }
}
