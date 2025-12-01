using System.Collections.Generic;
using UnityEngine;
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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

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

        if (saveables.Count > 0)
        {
            LoadGame();
        }

        else
        {
            Debug.Log("No saveables found; skipping load.");
        }
    }

    public void ClearSaveData()
    {
        SaveSystem.DeleteSave();
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
        if (shouldAutoSave)
        {
            autoSaveTimer += Time.deltaTime;
        }
        else if (autoSaveTimer != 0f)
        {
            autoSaveTimer = 0f;
        }

        if (autoSaveTimer >= autoSaveInterval && SceneManager.GetActiveScene().name != "MainMenu")
        {
            SaveGame();
            Debug.Log("Game auto-saved.");
            autoSaveTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.F5) && SceneManager.GetActiveScene().name == "MainMenu")
        {
            ClearSaveData();
            Debug.Log("Save data cleared via F5 key.");
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void SaveGame()
    {
        if (isSaving) return;
        isSaving = true;

        SaveData data = SaveSystem.Load() ?? new SaveData();

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

        SaveSystem.Save(data);
        isSaving = false;
    }

    public void LoadGame()
    {
        SaveData data = SaveSystem.Load();

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
        }

        foreach (ISaveable saveable in saveables)
        {
            if (saveable is PlayerController player)
            {
                player.LoadFrom(data);
                break;
            }
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.EnsureActiveObjective();
        }
    }

    public bool SaveExists()
    {
        return SaveSystem.SaveExists();
    }
}
