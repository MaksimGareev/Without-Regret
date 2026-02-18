using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Objectives / Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Objective Info")]
    [Tooltip("Unique identifier for the objective. This is used by the Objective Manager to track progress and completion of objectives. This should be unique for each objective in the game.")]
    public string objectiveID; // Unique identifier for the objective manager

    [Tooltip("Title of the objective. This is used for display purposes in the Journal UI and the Objective Popup UI.")]
    public string title; // Title shown in the Journal UI

    [Tooltip("Description of the objective. This is used for display purposes in the Journal UI.")]
    [TextArea] public string description; // Description shown in the Journal UI

    [Tooltip("Dialogue shown by Chime's Hint UI for this objective. This is used to give the player hints or additional information about the objective when they press the ChimeHint Button.")]
    [TextArea] public string chimeDialogue; // Dialogue shown by Chime's Hint UI for this objective

    [Header("Progress")]
    [Tooltip("How much progress the player needs to make to complete the objective. This is used by the Objective Manager to determing if the player has performed the correct amount of actions to complete the objective. For example, the gravestone objective would require 3 to complete since there are 3 gravestones to place, while an objective to talk to an NPC or travel to a location would only require 1.")]
    public int requiredProgress; // Amount of progress needed to complete the objective

    [Header("Scene")]
    [Tooltip("Name of the scene where the objective takes place.")]
    public string sceneName; // Name of the scene where the objective takes place
}
