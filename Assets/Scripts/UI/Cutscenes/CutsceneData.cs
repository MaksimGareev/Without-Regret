using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Cutscene/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    [Header("Cutscene Clips")]
    public CutsceneClip[] clips;
    
    [Tooltip("Whether or not the entire cutscene is able to be skipped.")]
    public bool canSkipEntireCutscene;

    [Tooltip("The audio clip to be played during the cutscene. This will be looped for the entire duration of the cutscene.")]
    public AudioClip backgroundMusic;
}

[System.Serializable]
public struct CutsceneClip
{
    [Header("Clip Settings")]
    [Tooltip("The image to be displayed during this cutscene clip.")]
    public Sprite backgroundImage;
    
    [Tooltip("The time in seconds that this clip will be shown for. If Auto Continue is true, this value will be the time when the next clip will play. If Auto Continue if false, this value will be the time when the player is able to press the continue button to manually trigger the next clip.")]
    public float duration;
    
    [Tooltip("Whether the clip will play the next clip automatically or require the player to manually press the continue button")]
    public bool autoContinue;
    
    [Tooltip("Whether or not the individual clip is able to be skipped before fully completed")]
    public bool canSkipClipEarly;
    
    [Tooltip("The dialogue line to play during this clip of the cutscene")]
    public CutsceneDialogueLine dialogueLine;
    
    [Tooltip("The audio clip to be played during this individual clip. Will only play once at the beginning of the clip")]
    public AudioClip clip;
    
    [Header("Events")]
    [Tooltip("Any events that should be triggered at the end of the clip. This can be used to trigger things like animations, sound effects, or other in-game events that should happen at specific points during the cutscene.")]
    public UnityEvent onClipCompleted;
}

[System.Serializable]
public class CutsceneDialogueLine
{
    [Tooltip("The name of the Speaker of the current line of dialogue")]
    public string Speaker;
    
    [Tooltip("Identify what audio mixer for the speakers gender")]
    public string NPCGender;
    
    [Tooltip("The text to display for the actual dialogue")]
    public string text;
}


