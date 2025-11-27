using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private List<ISaveable> saveables = new List<ISaveable>();
    public static SaveManager Instance;
    private float autoSaveInterval = 300f; // Auto-save every 5 minutes
    private float autoSaveTimer = 0f;

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

        if (scene.name != "MainMenu" && saveables.Count > 0)
        {
            LoadGame();
        }
        else
        {
            Debug.Log("No saveables found or in MainMenu scene; skipping load.");
        }
    }

    public void ClearSaveData()
    {
        SaveSystem.DeleteSave();
        Debug.Log("All save data cleared.");
    }

    private void RefreshSaveables()
    {
        saveables.Clear();

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

            if (!scene.IsValid() || scene != activeScene || mb.hideFlags != HideFlags.None)
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

    private void Update()
    {
        autoSaveTimer += Time.deltaTime;

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
        SaveData data = SaveSystem.Load() ?? new SaveData();

        data.lastSceneName = SceneManager.GetActiveScene().name;

        RefreshSaveables();

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
    }

    public bool SaveExists()
    {
        return SaveSystem.SaveExists();
    }
}
