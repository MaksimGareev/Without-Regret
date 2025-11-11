using UnityEngine;

[System.Serializable]
public class ObjectiveInstance : MonoBehaviour
{
    public ObjectiveData data;
    public int currentProgress;
    public bool isCompleted => currentProgress >= data.requiredProgress;

    public ObjectiveInstance(ObjectiveData data)
    {
        this.data = data;
        currentProgress = 0;
    }

    public void AddProgress(int amount)
    {
        currentProgress += amount;
        
        if (currentProgress > data.requiredProgress)
        {
            currentProgress = data.requiredProgress;
        }
    }
}
