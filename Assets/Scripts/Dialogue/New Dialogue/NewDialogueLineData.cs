using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NewDialogueLineData
{
    public string LineID;
    public string Speaker;

    [TextArea(3, 6)]
    public string text;

    public LineTone lineTone;
    public string NPCGender;

    public string NextLineID;
    public bool endDialogueAfterLine;
    public bool GiveItem;

    public List<NewDialogueChoiceData> choices;
    public List<string> objectivesToActivate;
}
