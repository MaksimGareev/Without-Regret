using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private List<ISaveable> saveables = new List<ISaveable>();
    private List<ISaveable> persistentSaveables = new List<ISaveable>();
    
    public bool shouldAutoSave = true;
    private float autoSaveInterval = 300f; // Auto-save every 5 minutes
    private float autoSaveTimer = 0f;
    private bool isSaving = false;

    // Dictionary to store unlock states
    private Dictionary<string, bool> unlockedItems = new Dictionary<string, bool>();

    public InputActionAsset inputActions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // public bool IsUnlocked(string itemName)
    // {
    //     if (unlockedItems.ContainsKey(itemName))
    //         return unlockedItems[itemName];
    //     return false;
    // }

    // public void SetUnlocked(string itemName, bool unlocked)
    // {
    //     if (unlockedItems.ContainsKey(itemName))
    //         unlockedItems[itemName] = unlocked;
    //     else
    //         unlockedItems.Add(itemName, unlocked);
    // }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSaveables();

        // if (saveables.Count > 0)
        // {
        //     LoadGame(SaveSystem.activeSaveSlot);
        // }

        // else
        // {
        //     Debug.Log("No saveables found; skipping load.");
        // }

        inputActions.FindActionMap("Player").Enable();
    }

    public void ClearSaveData(int slot)
    {
        SaveSystem.DeleteSave(slot);
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.ClearObjectivesOnDelete();
        }
        Debug.Log("All save data cleared.");
    }

    private void RefreshSaveables()
    {
        //var keep = new List<ISaveable>(saveables);
        saveables.Clear();
        saveables.AddRange(persistentSaveables);

        MonoBehaviour[] monoBehaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        var activeScene = SceneManager.GetActiveScene();
        
        foreach (var mb in monoBehaviours)
        {
            if (mb == null)
            {
                continue;
            }

            var go = mb.gameObject;
            var scene = go.scene;

            bool isInScene = scene == activeScene;
            bool isPersistent = scene.name == null || mb.gameObject.scene.name == "";

            if (!(isInScene || isPersistent) || mb.hideFlags != HideFlags.None)
            {
                continue;
            }

            if (mb is ISaveable saveable)
            {
                if (mb.gameObject.scene.isLoaded)
                {
                    // Ensure the saveable has a valid unique ID
                    if (saveable is SaveableFriendlyNPC fnpc)
                    {
                        if (string.IsNullOrEmpty(fnpc.GetUniqueID())) continue;
                    }
                    if (saveable is SaveableEnemyNPC enpc)
                    {
                        if (string.IsNullOrEmpty(enpc.GetUniqueID())) continue;
                    }
                    if (saveable is SaveableWorldObject wo)
                    {
                        if (string.IsNullOrEmpty(wo.GetUniqueID())) continue;
                    }
                    if (saveable is Gate gate)
                    {
                        if (string.IsNullOrEmpty(gate.GetUniqueID())) continue;
                    }

                    saveables.Add(saveable);
                }
            }
        }

        foreach (var saveable in saveables)
        {
            Debug.Log($"Found ISaveable: {saveable.GetType().Name} in scene: {activeScene.name}");
            if (saveable is SaveableWorldObject swo)
            {
                Debug.Log($"SaveableWorldObject ID: {swo.GetUniqueID()}");
            }
            if (saveable is SaveableFriendlyNPC fnpc)
            {
                Debug.Log($"SaveableFriendlyNPC ID: {fnpc.GetUniqueID()}");
            }
            if (saveable is SaveableEnemyNPC enpc)
            {
                Debug.Log($"SaveableEnemyNPC ID: {enpc.GetUniqueID()}");
            }
            if (saveable is Gate gate)
            {
                Debug.Log($"Gate ID: {gate.GetUniqueID()}");
            }
        }
    }

    public void RegisterSaveable(ISaveable saveable)
    {
        if (!persistentSaveables.Contains(saveable))
        {
            persistentSaveables.Add(saveable);
            Debug.Log("saveable object added to persistentSaveables");
        }

        if (!saveables.Contains(saveable))
        {
            saveables.Add(saveable);
            Debug.Log("saveable object added to saveables");
        }
        else
        {
            Debug.Log("object is not saveable!");
        }
    }

    public void RemoveSaveable(ISaveable saveable)
    {
        if (persistentSaveables.Contains(saveable))
        {
            persistentSaveables.Remove(saveable);
        }

        if (saveables.Contains(saveable))
        {
            saveables.Remove(saveable);
        }
    }

    private void Update()
    {
        if (shouldAutoSave && GameManager.Instance != null && GameManager.Instance.instanceReady)
        {
            autoSaveTimer += Time.deltaTime;
        }
        else if (autoSaveTimer != 0f)
        {
            autoSaveTimer = 0f;
        }

        if (autoSaveTimer >= autoSaveInterval && SceneManager.GetActiveScene().name != "MainMenu")
        {
            SaveGame(SaveSystem.activeSaveSlot);
            Debug.Log("Game auto-saved.");
            autoSaveTimer = 0f;
        }

        // if (Input.GetKeyDown(KeyCode.F5) && SceneManager.GetActiveScene().name == "MainMenu")
        // {
        //     ClearSaveData();
        //     Debug.Log("Save data cleared via F5 key.");
        //     SceneManager.LoadScene("MainMenu");
        // }
    }

    public void SaveGame(int slot)
    {
        if (isSaving) return;
        isSaving = true;

        SaveData data = SaveSystem.Load(slot) ?? new SaveData(slot);

        data.lastSceneName = SceneManager.GetActiveScene().name;

        RefreshSaveables();

        Debug.Log($"[SaveManager.SaveGame] Saveables count = {saveables.Count}");

        foreach (ISaveable saveable in saveables)
        {
            Debug.Log($"[SaveManager.SaveGame] calling SaveTo on {saveable.GetType().Name}");
            saveable.SaveTo(data);
        }

        int objCount = data.objectiveSaveData?.objectives?.Count ?? -1;
        Debug.Log($"[SaveManager.SaveGame] objectiveSaveData.objectives.Count = {objCount}");

        SaveSystem.Save(data, slot);
        isSaving = false;
    }

    public void LoadGame(int slot)
    {
        SaveData data = SaveSystem.Load(slot)?? new SaveData(slot);

        if (data == null)
        {
            Debug.LogWarning("No save data found to load.");
            return;
        }

        RefreshSaveables();

        foreach (ISaveable saveable in saveables)
        {
            if (saveable is PlayerController)
            {
                continue;
            }
            saveable.LoadFrom(data);
            Debug.Log($"[SaveManager.LoadGame] Loading {saveable.GetType().Name}");
        }

        foreach (ISaveable saveable in saveables)
        {
            if (saveable is PlayerController player && player.gameObject.GetComponentInParent<GameManager>() != null)
            {
                player.LoadFrom(data);
                Debug.Log($"[SaveManager.LoadGame] Loading PlayerController");
                break;
            }
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.EnsureActiveObjective();
        }

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ResumeGame();
        }
    }

    public void SetActiveSaveSlot(int slot)
    {
        SaveSystem.SetActiveSaveSlot(slot);
    }

    public bool AnySavesExist()
    {
        for (int slot = 1; slot <= 3; slot++)
        {
            if (SaveSystem.SaveExists(SaveSystem.GetSavePath(slot)))
            {
                return true;
            }
        }
        return false;
    }

    public bool SaveExists(int slot)
    {
        return SaveSystem.SaveExists(SaveSystem.GetSavePath(slot));
    }
}
