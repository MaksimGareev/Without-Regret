using System;
using UnityEngine;

public class SaveableWorldObject : MonoBehaviour, ISaveable
{
    [SerializeField] private string uniqueID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }

    public string GetUniqueID() => uniqueID;

    public void SaveTo(SaveData data)
    {
        WorldObjectState state = new WorldObjectState();

        state.id = uniqueID;
        state.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        state.rotation = new float[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        state.isActive = gameObject.activeSelf;

        if (GetComponent<WorldItem>() != null)
        {
            state.objectType = ObjectType.InventoryItem;
            state.hasBeenCollected = GetComponent<WorldItem>()?.hasBeenCollected ?? false;
            state.isCollectible = GetComponent<WorldItem>()?.isCollectible ?? true;
        }   
        else if (GetComponent<MoveableObject>() != null)
        {
            state.objectType = ObjectType.MovableObject;
            state.isGrabbable = GetComponent<MoveableObject>()?.isGrabbable ?? false;
        }
        else if (GetComponent<LockedItem>() != null)
        {
            state.objectType = ObjectType.LockpickableObject;
            state.hasBeenLockpicked = GetComponent<LockedItem>()?.hasBeenLockpicked ?? false;
        }
        else
        {
            state.objectType = ObjectType.MiscObject;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            state.rbConstraints = rb.constraints;
        }
        else
        {
            state.rbConstraints = RigidbodyConstraints.None;
        }

        if (data.worldSaveData.worldObjects.Exists(obj => obj.id == uniqueID))
        {
            data.worldSaveData.worldObjects.RemoveAll(obj => obj.id == uniqueID);
        }

        data.worldSaveData.worldObjects.Add(state);
    }

    public void LoadFrom(SaveData data)
    {
        var state = data.worldSaveData.worldObjects.Find(obj => obj.id == uniqueID);

        if (state == null)
        {
            Debug.LogWarning("No saved state found for object with ID: " + uniqueID);
            return;
        }

        transform.position = new Vector3(state.position[0], state.position[1], state.position[2]);
        transform.eulerAngles = new Vector3(state.rotation[0], state.rotation[1], state.rotation[2]);
        gameObject.SetActive(state.isActive);

        if (GetComponent<WorldItem>() != null && state.objectType == ObjectType.InventoryItem)
        {
            GetComponent<WorldItem>().hasBeenCollected = state.hasBeenCollected;
        }
        else if (GetComponent<MoveableObject>() != null && state.objectType == ObjectType.MovableObject)
        {
            GetComponent<MoveableObject>().isGrabbable = state.isGrabbable;
        }
        else if (GetComponent<LockedItem>() != null && state.objectType == ObjectType.LockpickableObject)
        {
            GetComponent<LockedItem>().hasBeenLockpicked = state.hasBeenLockpicked;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = state.rbConstraints;
        }
    }
}
