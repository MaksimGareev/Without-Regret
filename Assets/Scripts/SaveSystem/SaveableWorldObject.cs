using System;
using UnityEngine;

public class SaveableWorldObject : MonoBehaviour, ISaveable
{
    public string uniqueID = Guid.NewGuid().ToString();

    public void SaveTo(SaveData data)
    {
        WorldObjectState state = new WorldObjectState();

        state.id = uniqueID;
        state.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        state.rotation = new float[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        state.isActive = gameObject.activeSelf;

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
    }
}
