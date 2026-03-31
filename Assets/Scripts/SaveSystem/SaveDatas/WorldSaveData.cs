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
    OrbitingPlatform,
    RemoveableObject,
    MiscObject
}

[System.Serializable]
public class WorldObjectState
{
    // General
    public ObjectType objectType;
    public string id;
    public float[] position = new float[3];
    public float[] rotation = new float[3];
    public bool isActive;
    
    // Movable Item
    public bool isGrabbable;
    public RigidbodyConstraints rbConstraints;
    
    // Locked Item
    public bool hasBeenLockpicked;
    
    // Inventory World Item
    public bool hasBeenCollected;
    public bool isCollectible;
    
    // Orbiting platform
    public bool reachedLocation;
    
    // Removeable Object
    public bool interactable;
}
