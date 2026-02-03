using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldSaveData
{
    public List<WorldObjectState> worldObjects = new List<WorldObjectState>();
}

public enum ObjectType
{
    InventoryItem,
    MovableObject,
    LockpickableObject,
    MiscObject
}

[System.Serializable]
public class WorldObjectState
{
    public string id;
    public float[] position = new float[3];
    public float[] rotation = new float[3];
    public bool isActive;
    public ObjectType objectType;
    public bool isGrabbable;
    public RigidbodyConstraints rbConstraints;
    public bool hasBeenCollected;
    public bool hasBeenLockpicked;
    public bool isCollectible;
}
