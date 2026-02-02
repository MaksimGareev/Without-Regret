using System.Collections.Generic;

[System.Serializable]
public class ObjectiveRecord
{
    public string objectiveID;
    public string objectiveName;
    public int progress;
    public bool isCompleted;
}

[System.Serializable]
public class ObjectiveSaveData
{
    public List<ObjectiveRecord> objectives = new List<ObjectiveRecord>();
    public int currentObjectiveIndex = 0;
}
