using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectPool
{
    public readonly GameObject Prefab;
    public readonly Queue<GameObject> Pool;
    private readonly bool showDebugLogs;

    private readonly GameObject objectHolder;

    public ObjectPool (GameObject prefab, int initialSize, bool showDebugLogs = false, Transform location = null)
    {
        this.Prefab = prefab;
        this.Pool = new Queue<GameObject>();
        this.showDebugLogs = showDebugLogs;

        objectHolder = new GameObject(prefab.name + " Pool");

        for (int i = 0; i < initialSize; i++)
        {
            Vector3 spawnPos = location != null ? location.position : Vector3.zero;
            GameObject obj = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity, objectHolder.transform);
            obj.SetActive(false);
            Pool.Enqueue(obj);
        }
        if (showDebugLogs)
        {
            Debug.Log($"Initialized pool \"{objectHolder.name}\" for {prefab.name} with {Pool.Count} objects.");
        }
    }

    public GameObject Get(Transform parent = null)
    {
        GameObject obj;
        if (Pool.Count > 0)
        {
            obj = Pool.Dequeue();
            if (showDebugLogs)
            {
                Debug.Log($"Getting object from {objectHolder.name}: {obj.name}. Pool size is now {Pool.Count}.");
            }
            obj.SetActive(true);

            if (parent != null)
                obj.transform.SetParent(parent);

            return obj;
        }
        else
        {
            Transform newParent = parent != null ? parent : objectHolder.transform;
            obj = GameObject.Instantiate(Prefab, newParent);

            if (showDebugLogs)
                Debug.Log($"{objectHolder.name} is empty. Instantiating new object: {obj.name}.");

            obj.SetActive(true);
            return obj;
        }
    }
    public GameObject Get(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        GameObject obj;
        if (Pool.Count > 0)
        {
            obj = Pool.Dequeue();
            if (showDebugLogs)
            {
                Debug.Log($"Getting object from {objectHolder.name}: {obj.name}. Pool size is now {Pool.Count}.");
            }
            obj.SetActive(true);

            if (parent != null)
                obj.transform.SetParent(parent);

            obj.transform.SetPositionAndRotation(pos, rot);

            return obj;
        }
        else
        {
            Transform newParent = parent != null ? parent : objectHolder.transform;
            obj = GameObject.Instantiate(Prefab, newParent);

            if (showDebugLogs)
                Debug.Log($"{objectHolder.name} is empty. Instantiating new object: {obj.name}.");

            obj.SetActive(true);
            obj.transform.SetPositionAndRotation(pos, rot);

            return obj;
        }
    }


    public void Return(GameObject obj)
    {
        // Deactivate the object and return it to the pool.
        if (showDebugLogs)
        {
            Debug.Log($"Returning {obj.name} to {objectHolder.name}. Pool size before return: {Pool.Count}.");
        }
        obj.SetActive(false);
        if (obj.transform.parent != objectHolder.transform)
        {
            obj.transform.SetParent(objectHolder.transform);
        }
        Pool.Enqueue(obj);
    }
}
