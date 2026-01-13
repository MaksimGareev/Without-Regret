using UnityEngine;

[System.Serializable]
public class ObjectiveInstance
{
    public ObjectiveData data;
    public int currentProgress;
    public bool isCompleted;

    public ObjectiveInstance(ObjectiveData data)
    {
        this.data = data;
        currentProgress = 0;
        isCompleted = false;
    }

    public void AddProgress(int amount)
    {
        currentProgress += amount;

        if (currentProgress >= data.requiredProgress)
        {
            currentProgress = data.requiredProgress;
            isCompleted = true;
        }
    }

    public void SetProgress(int amount)
    {
        currentProgress = 0;
        AddProgress(amount);
    }
}
