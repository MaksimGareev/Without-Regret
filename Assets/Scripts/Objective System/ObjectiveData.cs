using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Objectives / Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Objective Info")]
    public string objectiveID;
    public string title;
    [TextArea] public string description;

    [Header("Progress")]
    public int currentProgress;
    public int requiredProgress;

    [Header("Status")]
    public bool isCompleted;
    public float ProgressPercent => (float)currentProgress / requiredProgress;

    public void ResetProgress()
    {
        currentProgress = 0;
        isCompleted = false;
    }
}
