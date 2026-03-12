using UnityEngine;

public abstract class SaveableWithID : MonoBehaviour, ISaveable
{
    [SerializeField, HideInInspector] private string uniqueID;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (string.IsNullOrEmpty(uniqueID) || !IsUniqueID(uniqueID))
        {
            RefreshUniqueID();
        }
    }

    private static bool IsUniqueID(string id)
    {
        var allSaveables = FindObjectsByType<SaveableWithID>(FindObjectsSortMode.None);
        foreach (var saveable in allSaveables)
        {
            if (saveable != null && saveable.GetUniqueID() == id)
            {
                return false;
            }
        }
        return true;
    }

    public void RefreshUniqueID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
        
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
    #endif

    public string GetUniqueID() => uniqueID;

    public abstract void SaveTo(SaveData data);

    public abstract void LoadFrom(SaveData data);
}
