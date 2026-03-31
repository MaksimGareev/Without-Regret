using System.Collections.Generic;

[System.Serializable]
public struct CharacterEntry
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
public struct CollectibleEntry
{
    public string collectibleName;
    public string description;

    public CollectibleEntry(string collectibleName, string description)
    {
        this.collectibleName = collectibleName;
        this.description = description;
    }
}

[System.Serializable]
public class JournalSaveData
{
    public List<CharacterEntry> characterEntryList = new();
    public List<CollectibleEntry> collectibleEntryList = new();
}