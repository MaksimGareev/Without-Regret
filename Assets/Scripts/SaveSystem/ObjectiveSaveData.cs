using System.Collections.Generic;

[System.Serializable]
public class ObjectiveSaveData
{
    public int currentObjectiveIndex = 0;
    public List<ObjectiveInstance> completedObjectives = new List<ObjectiveInstance>();
    public List<ObjectiveInstance> activeObjectives = new List<ObjectiveInstance>();
}
