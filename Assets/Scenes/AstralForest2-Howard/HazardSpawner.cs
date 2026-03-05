using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    public GameObject[] hazardPrefabs;
    public Transform[] spawnPoints;

    public void SpawnHazards()
    {
        if (hazardPrefabs == null || hazardPrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        foreach (var point in spawnPoints)
        {
            var prefab = hazardPrefabs[Random.Range(0, hazardPrefabs.Length)];
            Instantiate(prefab, point.position, point.rotation);
        }
    }
}