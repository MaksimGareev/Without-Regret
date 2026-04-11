using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Cutscene/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    [Header("Cutscene Clips")] public CutsceneClip[] clips;

    [Tooltip("Whether or not the entire cutscene is able to be skipped.")]
    public bool canSkipEntireCutscene;

    [Tooltip("The audio clip to be played during the cutscene. This will be looped for the entire duration of the cutscene.")]
    public AudioClip backgroundMusic;
    
    [Tooltip("The volume of the background music to be played during the cutscene. Will be ignored if there is no background music to play.")]
    [Range(0.0f, 1.0f)] public float musicVolume = 1.0f;

    [Header("Events")]
    [Tooltip("Any events that should be triggered at the end of the cutscene. This can be used to trigger things that should happen once the cutscene has finished.")]
    public UnityEvent onCutsceneCompleted;
}

[System.Serializable]
public class CutsceneClip
{
    [Header("Clip Settings")]
    [Tooltip("The image to be displayed during this cutscene clip.")]
    public Texture backgroundImage;
    
    [Tooltip("If true, this clip will display a solid color and ignore the background image set above. It will also use the color value set below.")]
    public bool useSolidColor;
    
    [Tooltip("If Use Solid Color is true, this will be the color that is displayed for the background of this clip.")]
    public Color solidColor;
    
    [Tooltip("The time in seconds that this clip will be shown for. If Auto Continue is true, this value will be the time when the next clip will play. If Auto Continue if false, this value will be the time when the player is able to press the continue button to manually trigger the next clip.")]
    public float duration = 5.0f;
    
    [Tooltip("Whether the clip will play the next clip automatically or require the player to manually press the continue button")]
    public bool autoContinue;
    
    [Tooltip("Whether or not the individual clip is able to be skipped before fully completed")]
    public bool canSkipClipEarly;
    
    [Tooltip("The dialogue line to play during this clip of the cutscene")]
    public CutsceneDialogueLine dialogueLine;
    
    [Tooltip("The audio clip to be played during this individual clip. Will only play once at the beginning of the clip")]
    public AudioClip clipSoundEffect;
    
    [Tooltip("The volume of the audio clip to be played in this individual clip. Will be ignored if there is no sound effect to play")]
    [Range(0.0f, 1.0f)] public float soundEffectVolume = 1.0f;
}

[System.Serializable]
public class CutsceneDialogueLine
{
    [Tooltip("The name of the Speaker of the current line of dialogue")]
    public string Speaker;
    
    [Tooltip("Identify what audio mixer for the speakers gender")]
    public CutsceneDialogueGender NPCGender;
    
    [Tooltip("The text to display for the actual dialogue")]
    [TextArea(3,6)] public string text;
}

[System.Serializable]
public enum CutsceneDialogueGender
{
    Male,
    Female,
    NonBinary
}


