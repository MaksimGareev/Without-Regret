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
    public string LineID;                       // Id of the current line of dialogue
    public string Speaker;                      // Name of the speaker of the current line of dialogue
    public string text;                         // text of the dialogue line
    public int requiredMorality;
    public string NextLineID;                   // the id of the next line of dialogue if there are no choices to choose from
    public List<DialogueChoice> choices;        // list of choices
    public List<string> objectivesToActivate;   // assign an objective to the player after engaging in dialogue
    public bool endDialogueAfterLine;           // allows line to end dialogue after being said
}

[System.Serializable]
public class DialogueChoice
{
    public string text;                 // text of the answer choice
    public string NextLineID;           // index of the next dialogue line
    public int moralityChange;          // change to the players morality
    public string objectiveToActivate;  // add objective 
    public string directionString;      // direction of answer choice
    
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

