using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string savePath = Application.persistentDataPath + "/save.json";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved to " + savePath);
    }

    public static SaveData Load()
    {
        if (!SaveExists())
        {
            Debug.LogWarning("Save file not found at " + savePath);
            return null;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("Game loaded from " + savePath);
        return data;
    }

    public static bool SaveExists()
    {
        return File.Exists(savePath);
    }
}
