using UnityEngine;

[System.Serializable]
public class NewDialogueChoiceData
{
    [TextArea(2, 4)]
    public string text;

    public string NextLineID;
    public int moralityChange;
    public ChoiceDirection direction;
}
