using System.Collections.Generic;

[System.Serializable]
public class ObjectiveSaveData
{
    public int currentObjectiveIndex;
    public List<string> completedObjectives = new List<string>();
    public List<string> activeObjectives = new List<string>();
}
