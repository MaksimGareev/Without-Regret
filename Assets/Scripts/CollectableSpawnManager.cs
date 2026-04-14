using UnityEngine;
using System.Collections.Generic;

public class CollectableSpawnManager : MonoBehaviour
{
    public static CollectableSpawnManager Instance;

    private Dictionary<string, Transform> spawnPoints = new Dictionary<string, Transform>(); 

    // Tracks spawned collectables
    private HashSet<string> spawnedIDs = new HashSet<string>();

    private void Awake()
    {
       if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Find all spawn points in scene
        CollectableSpawnPoint[] points = FindObjectsOfType<CollectableSpawnPoint>();

        foreach (var p in points)
        {
            if (!spawnPoints.ContainsKey(p.spawnPointID))
            {
                spawnPoints.Add(p.spawnPointID, p.transform);
            }
        }

    }

    public void SpawnCollectable(string collectableID, string spawnPointID, GameObject prefab, CollectableEntries data)
    {
        // Prevent duplicate spawns
        if (spawnedIDs.Contains(collectableID))
        {
            Debug.Log($"Collectable {collectableID} already spawned");
            return;
        }

        if (prefab == null || spawnPointID == null)
        {
            Debug.LogWarning("Missing prefab or spawn point!");
            return;
        }

        Transform point = spawnPoints[spawnPointID];

        GameObject obj = Instantiate(prefab, point.position, point.rotation);

        CollectableItem item = obj.GetComponent<CollectableItem>();

        if (item != null)
        {
            item.Initialize(data, NewDialogueManager.Instance.playerMorality);
        }

        spawnedIDs.Add(collectableID);

        Debug.Log($"Spawned collectable: {collectableID}");
    }

    public bool HasSpawned(string id)
    {
        return spawnedIDs.Contains(id);
    }

    public List<string> GetSpawnedIDs()
    {
        return new List<string>(spawnedIDs);
    }

    public void LoadSpawnedIDs(List<string> ids)
    {
        spawnedIDs = new HashSet<string>(ids);
    }
}
