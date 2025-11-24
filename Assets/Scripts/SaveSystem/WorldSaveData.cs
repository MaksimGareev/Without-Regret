using System.Collections.Generic;

[System.Serializable]
public class WorldSaveData
{
    public List<WorldObjectState> worldObjects = new List<WorldObjectState>();
}

[System.Serializable]
public class WorldObjectState
{
    public string id;
    public float[] position = new float[3];
    public float[] rotation = new float[3];
    public bool isActive;
}
