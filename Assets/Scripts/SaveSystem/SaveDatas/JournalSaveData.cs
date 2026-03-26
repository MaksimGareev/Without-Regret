using System.Collections.Generic;
using JetBrains.Annotations;

[System.Serializable]
public class CharacterEntry
{
    public string characterName;
    public string description;

    public CharacterEntry(string characterName, string description)
    {
        this.characterName = characterName;
        this.description = description;
    }
}

[System.Serializable]
public class JournalSaveData
{
    public List<CharacterEntry> characterEntryList = new();
}