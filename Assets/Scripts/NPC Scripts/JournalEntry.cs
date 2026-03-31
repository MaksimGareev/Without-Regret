using UnityEngine;

// Primarily used to add NPC/collectable info to the journal
[CreateAssetMenu(menuName = "Journal Entry")]
public class JournalEntry : ScriptableObject
{
    public string entryTitle;
    [TextArea(6, 8)]
    public string entryDescription;
}
