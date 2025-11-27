using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Objectives / Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Objective Info")]
    public string objectiveID;
    public string title;
    [TextArea] public string description;

    [Header("Progress")]
    // public int currentProgress;
    public int requiredProgress;

    [Header("Scene")]
    public string sceneName;

    // [Header("Status")]
    // public bool isCompleted;
    // public float ProgressPercent => (float)currentProgress / requiredProgress;
}
