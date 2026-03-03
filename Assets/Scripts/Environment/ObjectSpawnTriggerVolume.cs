using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectSpawnTriggerVolume : MonoBehaviour
{
    [SerializeField, Tooltip("The objects in scene to enable when the player enters the trigger volume")]
    GameObject[] objectsToEnable;

    private Collider coll;

    private void Awake()
    {
        coll = GetComponent<Collider>();
        coll.isTrigger = true;
    }

    private void Start()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            // When the player enters the trigger volume, spawn the objects and disable the collider to prevent multiple spawns
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
                else
                {
                    Debug.LogError("One of the objects to enable in ObjectSpawnTriggerVolume is null!", this);
                }
            }
            coll.enabled = false;
        }
    }

    public void OnDrawGizmos()
    {
        // Visualize the trigger volume
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
