using UnityEngine;

// Primarily used to add collectable info to the journal
[CreateAssetMenu(menuName = "Collectable Entry")]
public class CollectableEntries : ScriptableObject
{
    public string entryTitle;
    [TextArea(6, 8)]
    public string description;              // The string that will be the description visable in the journal
    public string neutralDescription;       // The neutral version of the collectable
    public string negativeDescription;      // The negative version of the collectable
    public string positiveDescription;      // The positive version of the collectable

    public string GetDescriptionByMorality(float morality)
    {
        if (morality > 5)
            return positiveDescription;

        if (morality < -5)
            return negativeDescription;

        return neutralDescription;
    }

}
