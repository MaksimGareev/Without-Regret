using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class NewDialogueData : ScriptableObject
{
    public string npcName;
    public List<NewDialogueLineData> dialogueLines;
}
