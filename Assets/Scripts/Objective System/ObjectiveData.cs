using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Objectives / Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Objective Info")]
    public string objectiveID; // Unique identifier for the objective manager
    public string title; // Title shown in the Journal UI
    [TextArea] public string description; // Description shown in the Journal UI
    [TextArea] public string chimeDialogue; // Dialogue shown by Chime's Hint UI for this objective

    [Header("Progress")]
    public int requiredProgress; // Amount of progress needed to complete the objective

    [Header("Scene")]
    public string sceneName; // Name of the scene where the objective takes place
}
