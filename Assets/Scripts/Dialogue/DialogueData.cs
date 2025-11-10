using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueData
{
    public string npcName;
    public List<DialogueLine> dialogueLines; // create list of lines
}

[System.Serializable]
public class DialogueLine
{
    public string text;
    public List<DialogueChoice> choices; // list of choices
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public int nextIndex; // index of the next dialogue line

    public string morality;
    //public int currentValue;
    public int valueChange;
}

