using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string savePath1 = Application.persistentDataPath + "/save1.json";
    private static readonly string savePath2 = Application.persistentDataPath + "/save2.json";
    private static readonly string savePath3 = Application.persistentDataPath + "/save3.json";
    public static int activeSaveSlot = 1;

    public static void Save(SaveData data, int slot)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetSavePath(slot), json);
            Debug.Log("Game saved to " + GetSavePath(slot));
        }
        catch (Exception ex)
        {
            Debug.LogError("SaveFailed: " + ex);
        }
        
    }

    public static SaveData Load(int slot)
    {
        if (!SaveExists(GetSavePath(slot)))
        {
            Debug.LogWarning("Save file not found at " + GetSavePath(slot));
            return null;
        }

        string json = File.ReadAllText(GetSavePath(slot));
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("Game loaded from " + GetSavePath(slot));
        return data;
    }

    public static void DeleteSave(int slot)
    {
        if (SaveExists(GetSavePath(slot)))
        {
            File.Delete(GetSavePath(slot));
            Debug.Log("Save file deleted from " + GetSavePath(slot));
        }
        else
        {
            Debug.LogWarning("No save file to delete at " + GetSavePath(slot));
        }
    }

    public static void SetActiveSaveSlot(int slot)
    {
        activeSaveSlot = slot;
    }

    public static string GetSavePath(int slot)
    {
        switch (slot)
        {
            case 1:
                return savePath1;
            case 2:
                return savePath2;
            case 3:
                return savePath3;
            default:
                throw new ArgumentOutOfRangeException("Invalid save slot: " + slot);
        }
    }

    public static bool SaveExists(string path)
    {
        return File.Exists(path);
    }
}
