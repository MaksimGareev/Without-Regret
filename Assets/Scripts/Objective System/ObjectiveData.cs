using UnityEngine;

// This scriptable object holds the data for an objective, which can be used by the Objective Manager to track progress and completion of objectives. 
// It also contains information for display purposes in the Journal UI and the Objective Popup UI, as well as dialogue for Chime's Hint UI.
[CreateAssetMenu(fileName = "New Objective", menuName = "Objectives / Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Objective Info")]
    [Tooltip("Unique identifier for the objective. This is used by the Objective Manager to track progress and completion of objectives. This should be unique for each objective in the game.")]
    public string objectiveID; // Unique identifier for the objective manager

    [Tooltip("Title of the objective. This is used for display purposes in the Journal UI and the Objective Popup UI.")]
    public string title; // Title shown in the Journal UI

    [Tooltip("Description of the objective. This is used for display purposes in the Journal UI. It should be written as if the objective is currently active in order to guide the player towards completing the objective.")]
    [TextArea] public string description; // Description shown in the Journal UI for active objectives

    [Tooltip("Recap of the objective. This is used for display purposes in the Journal UI. It should be written as if the objective has already been completed in order to remind the player of what has already happened, and what choices they have made.")]
    [TextArea] public string recap; // Recap shown in the Journal UI for comleted objectives

    [Tooltip("Dialogue shown by Chime's Hint UI for this objective. This is used to give the player hints or additional information about the objective when they press the ChimeHint Button.")]
    [TextArea] public string chimeDialogue; // Dialogue shown by Chime's Hint UI for this objective

    [Header("Progress")]
    [Tooltip("How much progress the player needs to make to complete the objective. This is used by the Objective Manager to determing if the player has performed the correct amount of actions to complete the objective. For example, the gravestone objective would require 3 to complete since there are 3 gravestones to place, while an objective to talk to an NPC or travel to a location would only require 1.")]
    public int requiredProgress; // Amount of progress needed to complete the objective

    [Header("Scene")]
    [Tooltip("Build index of the scene where the objective takes place.")]
    public int sceneIndex; // build index of the scene where the objective takes place

    [Header("Objective Location")]
    [Tooltip("The ObjectiveMarker object you want the objective indicator to point to When this becomes the active objective.")]
    public Vector3 markerTransform; // Object that the objective indicator points to that also handles the on-screen quest marker

    [Tooltip("Turn off if you don't want the objective to have an in-world indicator.")]
    public bool hasMarker = true; // Whether or not the objective has an in-world marker

    [Tooltip("Turn off if you don't want the objctive to have an off-screen indicator.")]
    public bool hasOffScreenMarker = true; // Whether or not the objective has an off-screen marker

}
