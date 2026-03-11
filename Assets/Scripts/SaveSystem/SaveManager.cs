using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// Save Manager is responsible for managing the saving and loading of game data, as well as keeping track of all ISaveable objects in the scene.
// It uses the SaveSystem class to handle the actual file operations for saving and loading game data from JSON files.
// It acts as an intermediary between the saveable objects in the scene and the SaveSystem, allowing the object's relevant data to be saved and loaded appropriately.
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }  // Singleton instance of Savemanager for other scripts to access

    [Header("References")]
    [Tooltip("Reference to the TextMeshProUGUI component that will display saving status messages to the player. This should be a child of the SaveManager GameObject.")]
    [SerializeField] private TextMeshProUGUI savingText;

    [Tooltip("Reference to the InputActionAsset that contains the player's input actions. There should only be one InputActionAsset that exists in the project (PlayerControls)")]
    public InputActionAsset inputActions;

    [Header("Settings")]
    [Tooltip("If true, the game will automatically save at regular intervals. Auto-saving will only occur when the player is in a scene other than the Main Menu.")]
    public bool shouldAutoSave = true;

    [Tooltip("If true, the SaveManager will log detailed debug information about saveables and save/load operations to the console. This is used for debugging save/load issues during development, but should be set to false when not needed.")]
    public bool showDebugLogs = true;

    private List<ISaveable> saveables = new List<ISaveable>(); // List of all saveable objects in the current scene that will be saved and loaded by the SaveManager
    private List<ISaveable> persistentSaveables = new List<ISaveable>(); // List of saveables that are marked as Dont Destroy on Load and should persist across scene loads

    private float autoSaveInterval = 300f; // Auto-save every 5 minutes
    private float autoSaveTimer = 0f;  // Timer to track time since last auto-save
    private bool isSaving = false; // Flag to prevent multiple simultaneous save operations

    public bool IsLoading { get; private set; } = false;
    private void Awake()
    {
        // Implementing the singleton pattern to ensure only one instance of SaveManager exists and persists across scenes
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

    // When the SaveManager is enabled, subscribe to the sceneLoaded event
    private void OnEnable()
    {
        SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
    }

    // Unsubscribe from the sceneLoaded event when the SaveManager is disabled to prevent memory leaks and unintended behavior
    private void OnDisable()
    {
        SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(OnSceneLoaded);
    }

    // When a new scene is loaded, refresh the list of saveable objects and load the game from the active save slot if any save data exists.
    private void OnSceneLoaded()
    {
        RefreshSaveables();

        if (saveables.Count > 0)
        {
            LoadGame(SaveSystem.activeSaveSlot);
        }

        else if (showDebugLogs)
        {
            Debug.Log("No saveables found; skipping load.");
        }

        inputActions.FindActionMap("Player").Enable();
    }

    // Clears all save data for the specified slot by deleting the corresponding save file and clearing any relevant data in the ObjectiveManager. 
    // This is called when the player chooses to delete a save slot from the UI.
    public void ClearSaveData(int slot)
    {
        SaveSystem.DeleteSave(slot);

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.ClearObjectivesOnDelete();
        }

        if (showDebugLogs) Debug.Log("All save data cleared.");
    }

    // Refreshes the list of saveable objects in the current scene by finding all MonoBehaviours that implement the ISaveable interface
    // and are either in the active scene or marked as Dont Destroy on Load.
    private void RefreshSaveables()
    {
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

             // Ensure the saveable has a valid unique ID and exists in the currently loaded scene
             // if not then do not add it to saveables list
            if (mb is ISaveable saveable)
            {
                if (mb.gameObject.scene.isLoaded)
                {
                   
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

        // Log detailed information about the saveables found in the scene for debugging purposes 
        if (showDebugLogs)
        {
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
    }

    // Registers a saveable object with the SaveManager, adding it to the list of saveables that will be saved and loaded. 
    // If the saveable is marked as Dont Destroy on Load, it will also be added to the list of persistent saveables that are not cleared when loading new scenes.
    public void RegisterSaveable(ISaveable saveable)
    {
        if (!persistentSaveables.Contains(saveable))
        {
            persistentSaveables.Add(saveable);
            if (showDebugLogs) Debug.Log("saveable object added to persistentSaveables");
        }

        if (!saveables.Contains(saveable))
        {
            saveables.Add(saveable);
            if (showDebugLogs) Debug.Log("saveable object added to saveables");
        }
        else
        {
            if (showDebugLogs) Debug.Log("object is not saveable!");
        }
    }

    // Remove the saveable from both lists to ensure it is no longer saved or loaded (so it doesn't call save on objects not in the current scene)
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
        HandleAutoSave();

        // Debug key to clear save data (for testing purposes only)
        // if (Input.GetKeyDown(KeyCode.F5) && SceneManager.GetActiveScene().name == "MainMenu")
        // {
        //     ClearSaveData();
        //     Debug.Log("Save data cleared via F5 key.");
        //     SceneManager.LoadScene("MainMenu");
        // }
    }

    private void HandleAutoSave()
    {
        // Early return if auto-saving is disabled or if the player is in the Main Menu, and reset the auto-save timer to prevent unintended auto-saves when leaving the Main Menu
        if (!shouldAutoSave || SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (autoSaveTimer != 0f)
            {
                autoSaveTimer = 0f;
            }
            return;
        }
        
        autoSaveTimer += Time.deltaTime;

        // Auto-save the game at regular intervals and reset the timer for the next auto-save
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveGame(SaveSystem.activeSaveSlot);
            if (showDebugLogs) Debug.Log("Game auto-saved.");
            autoSaveTimer = 0f;
        }
    }

    // Saves the game by calling the SaveTo method on all registered ISaveable objects and writing the resulting SaveData to a file in the specified save slot.
    // If no save data exists for the specified slot, a new SaveData object will be created. 
    // The last scene name will also be saved to the SaveData to allow for loading back into the correct scene.
    public void SaveGame(int slot)
    {
        if (isSaving) return;
        isSaving = true;
        StartCoroutine(ShowSavingText());

        SaveData data = SaveSystem.Load(slot) ?? new SaveData(slot);

        data.lastSceneName = SceneManager.GetActiveScene().name;

        if (TimerRingUI.Instance != null && TimerRingUI.Instance.currentRingState != TimerRingUI.RingState.Empty)
        {
            data.playerSaveData.currentRingState = TimerRingUI.Instance.currentRingState;
        }
        else if (TimerRingUI.Instance == null || TimerRingUI.Instance.currentRingState == TimerRingUI.RingState.Empty)
        {
            data.playerSaveData.currentRingState = TimerRingUI.RingState.Full;
        }

        RefreshSaveables();

        if (showDebugLogs) Debug.Log($"[SaveManager.SaveGame] Saveables count = {saveables.Count}");

        // Loop through the saveables list and call each object's SaveTo method, passing in the SaveData to be written to.
        foreach (ISaveable saveable in saveables)
        {
            if (showDebugLogs) Debug.Log($"[SaveManager.SaveGame] calling SaveTo on {saveable.GetType().Name}");
            saveable.SaveTo(data);
        }

        if (showDebugLogs)
        {
            int objCount = data.objectiveSaveData?.objectives?.Count ?? -1;
            Debug.Log($"[SaveManager.SaveGame] objectiveSaveData.objectives.Count = {objCount}");
        }

        // Call to the SaveSystem to write the saved data to a JSON file in the appropriate save slot
        SaveSystem.Save(data, slot);
        isSaving = false;
    }

    // Coroutine to display a simple message to the player when the game is being saved.
    private IEnumerator ShowSavingText()
    {
        if (savingText != null)
        {
            savingText.gameObject.SetActive(true);
            savingText.text = "Saving...";

            yield return new WaitForSecondsRealtime(1f);
            savingText.text = "Progress Saved!";

            yield return new WaitForSecondsRealtime(2f);
            savingText.gameObject.SetActive(false);
        }
    }

    // Loads the game from the specified save slot and applies the loaded data to all ISaveable objects in the scene. 
    // If no save data is found for the specified slot, a warning will be logged and no loading will occur.
    public void LoadGame(int slot)
    {
        IsLoading = true;

        // Try loading from the specified save slot, if no save exists then create a new one in that slot.
        SaveData data = SaveSystem.Load(slot)?? new SaveData(slot);

        // Early return if no save data is found (if above line fails to create a new save data)
        if (data == null)
        {
            if (showDebugLogs) Debug.LogWarning("No save data found to load.");
            return;
        }

        RefreshSaveables();

        // Loop through the saveables list and call each object's LoadFrom method, passing in the loaded SaveData. 
        // This will allow each object to update its state based on the saved data.
        foreach (ISaveable saveable in saveables)
        {
            if (saveable is PlayerController)
            {
                continue;
            }
            saveable.LoadFrom(data);
            if (showDebugLogs) Debug.Log($"[SaveManager.LoadGame] Loading {saveable.GetType().Name}");
        }

        foreach (ISaveable saveable in saveables)
        {
            if (saveable is PlayerController player)
            {
                player.LoadFrom(data);
                if (showDebugLogs) Debug.Log($"[SaveManager.LoadGame] Loading PlayerController");
                break;
            }
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.EnsureActiveObjective();
        }
        
        // Set the timer ring state to the state in the saved data
        if (TimerRingUI.Instance != null && data.playerSaveData.currentRingState != TimerRingUI.RingState.Empty)
        {
            TimerRingUI.Instance.SetRingState(data.playerSaveData.currentRingState);
        }
        else if (TimerRingUI.Instance != null && data.playerSaveData.currentRingState == TimerRingUI.RingState.Empty)
        {
            TimerRingUI.Instance.SetRingState(TimerRingUI.RingState.Full);
        }

        // Ensure the game is not paused after loading, which can happen if the player saves while paused and then reloads that save
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ResumeGame();
        }

        IsLoading = false;
    }

    // Sets the active save slot number to the specified slot
    public void SetActiveSaveSlot(int slot)
    {
        SaveSystem.SetActiveSaveSlot(slot);
    }

    // Searches for a save file in all slots and returns true if at least one save file exist, false otherwise
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

    // Searches for a save file in the specified slot and returns true if it exists, false otherwise
    public bool SaveExists(int slot)
    {
        return SaveSystem.SaveExists(SaveSystem.GetSavePath(slot));
    }
}
