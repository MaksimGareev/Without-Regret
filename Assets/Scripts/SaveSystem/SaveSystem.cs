using System;
using System.IO;
using UnityEngine;

// Static class that provides methods for saving and loading game data to and from JSON files. 
// This class is used by the SaveManager to handle the actual file operations for saving and loading game data.
public static class SaveSystem
{
    // Static file paths for 3 save slots, using Application.persistentDataPath to ensure they are stored in a platform-appropriate location.
    // The file names indicate the corresponding save slot number.
    private static readonly string savePath1 = Application.persistentDataPath + "/save1.json";
    private static readonly string savePath2 = Application.persistentDataPath + "/save2.json";
    private static readonly string savePath3 = Application.persistentDataPath + "/save3.json";

    // Used to keep track of which save slot is currently active for saving and loading operations.
    // This can be set from the SaveManager or from the SaveSlotUI when the player selects a save slot.
    public static int activeSaveSlot = 1;  
    
    // Serializes the provided SaveData object into JSON format and writes it to a file corresponding to the specified save slot.
    // Primarily called from SaveManager when saving the game, so it can pass the data from the saveables to be serialized and written into the JSON.
    // If an error occurs during saving, an error message with the exception details is logged to the console.
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

    // Reads the JSON save file from the specified slot, deserializes it into a SaveData object, and returns that object.
    // Primarily called from SaveManager when loading a game, so it can pass the data from the JSON to the objects to update them correctly.
    // If no save file exists at the specified slot, a warning is logged to the console and null is returned.
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

    // Deletes the save file at the specified slot if it exists. If no save file exists at that slot, a warning is logged to the console.
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

    // Sets the active save slot number that will be used for saving and loading operations.
    public static void SetActiveSaveSlot(int slot)
    {
        activeSaveSlot = slot;
    }

    // Translates the save slot number (int) into the corresponding file path (string) for that slot and returns it
    // If an invalid slot number is provided, an exception is thrown
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

    // Checks if a save file exists at the specified path, returns the result as a boolean
    public static bool SaveExists(string path)
    {
        return File.Exists(path);
    }
}
