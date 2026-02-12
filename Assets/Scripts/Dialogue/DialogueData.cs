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
    public string Speaker;
    public string text;
    public int requiredMorality;
    public List<DialogueChoice> choices;
    public List<string> objectivesToActivate;// list of choices
    public bool endDialogueAfterLine; // allows line to end dialogue after being said
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public int nextIndex; // index of the next dialogue line
    public int moralityChange;
    public string objectiveToActivate; // add objective 

    public string directionString;
    [System.NonSerialized] public ChoiceDirection direction;

    public void ParseDirection()
    {
        if (System.Enum.TryParse(directionString, true, out ChoiceDirection dir))
        {
            direction = dir;
        }
        else
        {
            direction = ChoiceDirection.Up;
        }
    }
}

