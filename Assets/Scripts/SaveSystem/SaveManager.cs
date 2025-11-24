using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private ISaveable[] saveables;

    private void Awake()
    {
        saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID) as ISaveable[];
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        foreach (ISaveable saveable in saveables)
        {
            saveable.SaveTo(data);
        }

        SaveSystem.Save(data);
    }

    public void LoadGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            Debug.LogWarning("No save data found to load.");
            return;
        }

        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadFrom(data);
        }
    }
}
