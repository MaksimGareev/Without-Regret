using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private ISaveable[] saveables;
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
        saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID) as ISaveable[];

        if (scene.name != "MainMenu" && saveables != null)
        {
            LoadGame();
            SaveGame();
        }
        else
        {
            Debug.Log("No saveables found or in MainMenu scene; skipping load.");
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
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.lastSceneName = SceneManager.GetActiveScene().name;

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

    public bool SaveExists()
    {
        return SaveSystem.SaveExists();
    }
}
