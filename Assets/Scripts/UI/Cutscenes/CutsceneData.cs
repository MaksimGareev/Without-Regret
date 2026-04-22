using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Cutscene/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    [Header("General Settings")]
    [Tooltip("Whether or not the cutscene should fade in at the beginning. If false, the cutscene will start immediately with no fade in. This should be false if the cutscene happens between level transitions, like the intro and the transition to astral plane.")]
    public bool fadeIn = true;
    
    [Tooltip("Whether or not the entire cutscene is able to be skipped.")]
    public bool canSkipEntireCutscene;
    
    [Header("Cutscene Clips")] 
    [Tooltip("The individual clips that make up this cutscene. Each clip will be played in order, and will have its own settings for background, dialogue, and audio.")]
    public CutsceneClip[] clips;
    
    [Header("Audio Settings")]
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
    public enum BackgroundType
    {
        Image,
        ImageFromPreviousClip,
        SolidColor
    }
    
    [Header("Clip Settings")]
    [Tooltip("This will control which type of background will be shown for this clip. Selecting Image will use the image assigned to background image, and ignore the solid color. Image from previous clip will ignore both solid color and the background image, and simply use the image in the previous clip. Solid color will ignore the background image, and just display the color set in solid color.")]
    public BackgroundType backgroundType = BackgroundType.Image;
    
    [Tooltip("The image to be displayed during this cutscene clip.")]
    public Texture backgroundImage;
    
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


