using UnityEngine;

[System.Serializable]
public class ObjectiveInstance
{
    public ObjectiveData data; // Reference to the ObjectiveData scriptable object
    public int currentProgress; // Current progress towards completing the objective
    public bool isCompleted; // Whether the objective is completed

    // Constructor to initialize the ObjectiveInstance
    public ObjectiveInstance(ObjectiveData data)
    {
        this.data = data;
        currentProgress = 0;
        isCompleted = false;
    }

    // Adds progress to the objective and checks for completion
    public void AddProgress(int amount)
    {
        currentProgress += amount;

        if (currentProgress >= data.requiredProgress)
        {
            currentProgress = data.requiredProgress;
            isCompleted = true;
        }
    }

    // Sets the current progress to a specific amount, i.e. if an action has a set progress value other than incrementing by one
    public void SetProgress(int amount)
    {
        currentProgress = 0;
        AddProgress(amount);
    }
}
