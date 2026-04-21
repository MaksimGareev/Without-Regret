using UnityEngine;
using System.Collections.Generic;

public enum LineTone
{
    Neutral,    // 0 = Neutral
    Happy,      // 1 = Happy
    Upset       // 2 = Upset
}

[System.Serializable]
public class NewDialogueLineData
{
    public string LineID;   // Id of the current line
    public string Speaker;  // Who is talking in the current line

    [TextArea(3, 6)]
    public string text;     // Text for the current line

    public LineTone lineTone;   // Tone value for the portrait of the speaker
    public string NPCGender;    // Gender of the speaker that determines what audio mixer to use

    public string NextLineID;           // ID of the next line to move to
    public bool endDialogueAfterLine;   // End the dialogue after this line (used for moving NPCs to target points after interacting with them)
    public bool movingOn;               // Bool that controls if the moving on VFX should be played 
    public bool GiveItem;               // Bool that contorls if the NPC gives the player an item
    public bool ShakeCamera;            // Bool that contorls if the camera should shake durring the current line
    public bool cannotSkip;             // Bool that contorls if the player cannont skip this line of dialogue and should be fully built out

    [Header("Audio")]
    public bool playSFXOnstart;
    public AudioClip SFX;

    [Header("Collectable Spawn")]
    public bool spawnCollectible;
    public GameObject collectablePrefab;
    public string spawnPointID;
    public string collectableSpawnID;
    public CollectableEntries collectableData;

    public List<NewDialogueChoiceData> choices;
    public List<string> objectivesToActivate;
}
