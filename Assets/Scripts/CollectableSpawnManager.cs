using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CollectableSpawnManager : MonoBehaviour, ISaveable
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
        DontDestroyOnLoad(gameObject);

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebuildSpawnPoints(scene);
    }

    public void RebuildSpawnPoints(Scene scene)
    {
        spawnPoints.Clear();

        // Find all spawn points in scene
        CollectableSpawnPoint[] points = FindObjectsOfType<CollectableSpawnPoint>();

        Debug.Log($"[CollectableSpawnManager] Scene Loaded: {scene.name}");
        Debug.Log($"Spawn points found: {points.Length}");

        foreach (var p in points)
        {
            if (p == null) continue;

            if (string.IsNullOrEmpty(p.spawnPointID))
            {
                continue;
            }

            if (!spawnPoints.ContainsKey(p.spawnPointID))
            {
                spawnPoints.Add(p.spawnPointID, p.transform);
            }
            else
            {
                Debug.LogWarning($"Duplicate spawnpointID detected: {p.spawnPointID}");
            }
        }

        Debug.Log("Registered spawn points total: " + spawnPoints.Count);
    }

    public void SpawnCollectable(string collectableID, string spawnPointID, GameObject prefab, CollectableEntries data)
    {
        // Prevent duplicate spawns
        if (spawnedIDs.Contains(collectableID))
        {
            Debug.Log($"Collectable {collectableID} already spawned");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning("Missing prefab");
            return;
        }

        if (string.IsNullOrEmpty(spawnPointID))
        {
            Debug.LogWarning($"Missing SpawnPointID for collectable : {collectableID}");
            return;
        }

        if (data == null)
        {
            Debug.LogWarning($"SpawnPoint ID '{spawnPointID}' not found in scene!");
            return;
        }

        if (!spawnPoints.TryGetValue(spawnPointID, out Transform point))
        {
            Debug.LogError($"SpawnPoint ID '{spawnPointID}' not found in scene '{SceneManager.GetActiveScene().name}'!");
            return;
        }

        //Transform point = spawnPoints[spawnPointID];

        GameObject obj = Instantiate(prefab, point.position, point.rotation);

        CollectableItem item = obj.GetComponent<CollectableItem>();

        if (item != null)
        {
            int morality = 0;
            
            if (NewDialogueManager.Instance != null)
            {
                morality = NewDialogueManager.Instance.playerMorality;
            }

            item.Initialize(data, morality);
        }
        else
        {
            Debug.LogWarning($"Collectable component missing on prefab: {prefab.name}");
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

    public void SaveTo(SaveData data)
    {
        throw new System.NotImplementedException();
    }

    public void LoadFrom(SaveData data)
    {
        throw new System.NotImplementedException();
    }
}
