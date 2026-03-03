using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObjectiveAndSaveTesting : MonoBehaviour
{
    [System.Serializable]
    public struct SceneObjectivePair
    {
        public SceneReference scene;
        public ObjectiveData objectiveData;
    }

    [Header("Testing Settings")]
    [Tooltip("Mapping of scenes to their corresponding objectives. When a scene is loaded, the mapped objective will be activated.")]
    [SerializeField] private List<SceneObjectivePair> sceneObjectiveList;

    // Runtime dictionary for quick lookup of objectives based on scene name, populated on Awake from above List
    private Dictionary<string, ObjectiveData> sceneObjectiveMap = new Dictionary<string, ObjectiveData>();

    // Array to hold scene names for button listeners
    private string[] sceneNames;

    [Header("Input References")]
    [Tooltip("InputActionAsset reference for skipping objectives")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction cancelAction;
    private InputAction objectiveDebugUIAction;
    private InputAction skipObjectiveAction;

    [Header("Debug UI")]
    [SerializeField] private GameObject debugUI;
    [SerializeField] private Button[] LevelSelectButtons;
    [SerializeField] private Button CloseUIButton;

    private void Awake()
    {
        InitializeDataForButtons();
        InitializeInputActions();
    }

    private void InitializeDataForButtons()
    {
        // Populate the sceneObjectiveMap from the sceneObjectiveList
        foreach (var pair in sceneObjectiveList)
        {
            if (!sceneObjectiveMap.ContainsKey(pair.scene.GetSceneName()))
            {
                sceneObjectiveMap.Add(pair.scene.GetSceneName(), pair.objectiveData);
            }
            else
            {
                Debug.LogWarning($"Duplicate scene {pair.scene.GetSceneName()} in sceneObjectiveList");
            }
        }

        sceneNames = new string[sceneObjectiveList.Count];

        // Prepare scene names array for button listeners
        for (int i = 0; i < sceneObjectiveList.Count; i++)
        {
            sceneNames[i] = sceneObjectiveList[i].scene.GetSceneName();
        }
    }

    private void InitializeInputActions()
    {
        // Initialize the input action for opening the debug UI
        if (inputActions != null)
        {
            objectiveDebugUIAction = inputActions.FindAction("Debug/ObjectiveDebugUI");
            skipObjectiveAction = inputActions.FindAction("Debug/SkipObjective");
            cancelAction = inputActions.FindAction("UI/Cancel");

            if (objectiveDebugUIAction != null)
            {
                objectiveDebugUIAction.Enable();
            }
            else
            {
                Debug.LogWarning("ObjectiveDebugUI action not found in InputActionAsset");
            }

            if (skipObjectiveAction != null)
            {
                skipObjectiveAction.Enable();
            }
            else
            {
                Debug.LogWarning("SkipObjective action not found in InputActionAsset");
            }

            if (cancelAction != null)
            {
                cancelAction.Enable();
            }
            else            
            {
                Debug.LogWarning("Cancel action not found in InputActionAsset");
            }
        }
        else
        {
            Debug.LogWarning("InputActionAsset reference is missing");
        }
    }

    private void Update()
    {
        // Early returns to avoid unnecessary checks when conditions are not met
        if (sceneObjectiveMap.Count == 0 || objectiveDebugUIAction == null || debugUI == null) return;
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        if (Journal.Instance != null && Journal.Instance.isJournalOpen) return;
        if (PauseManager.Instance != null && PauseManager.Instance.isGamePaused) return;

        // Check for input to toggle debug UI
        if (objectiveDebugUIAction != null && objectiveDebugUIAction.triggered && !debugUI.activeSelf)
        {
            OpenDebugUI();
        }
        else if (((objectiveDebugUIAction != null && objectiveDebugUIAction.triggered) || (cancelAction != null && cancelAction.triggered)) && debugUI.activeSelf)
        {
            CloseDebugUI();
        }

        if (skipObjectiveAction != null && skipObjectiveAction.triggered)
        {
            SkipObjective();
        }
    }

    private void OpenDebugUI()
    {
        AddListeners();
        debugUI.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseDebugUI()
    {
        RemoveListeners();
        debugUI.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void AddListeners()
    {
        // Add listeners to level select buttons
        if (LevelSelectButtons != null && LevelSelectButtons.Length > 0)
        {
            for (int i = 0; i < sceneObjectiveList.Count; i++)
            {
                int index = i; // Capture the current value of i for the lambda
                LevelSelectButtons[i].onClick.AddListener(() => LoadScene(sceneNames[index]));
            }
        }
        else
        {
            Debug.LogWarning("LevelSelectButtons array is not set or empty");
        }

        if (CloseUIButton != null)
        {
            CloseUIButton.onClick.AddListener(CloseDebugUI);
        }
        else
        {
            Debug.LogWarning("CloseUIButton reference is missing");
        }
    }

    private void RemoveListeners()
    {
        for (int i = 0; i < sceneObjectiveList.Count - 1; i++)
        {
            int index = i; // Capture the current value of i for the lambda
            LevelSelectButtons[i].onClick.RemoveListener(() => LoadScene(sceneNames[index]));
        }

        if (CloseUIButton != null)
        {
            CloseUIButton.onClick.RemoveListener(CloseDebugUI);
        }
    }

    private void SkipObjective()
    {
        if (ObjectiveManager.Instance == null)
        {
            Debug.LogWarning("ObjectiveManager instance is not available");
            return;
        }

        var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
            
        foreach (var obj in activeObjectives)
        {
            ObjectiveManager.Instance.AddProgress(obj.data.objectiveID, 1);
        }
    }

    private void LoadScene(string sceneName)
    {
        if (ObjectiveManager.Instance == null)
        {
            Debug.LogWarning("ObjectiveManager instance is not available");
            return;
        }

        if (sceneObjectiveMap.ContainsKey(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            CloseDebugUI();
        }

        if (sceneObjectiveMap.TryGetValue(sceneName, out ObjectiveData objectiveData))
        {
            ObjectiveManager.Instance.SkipToObjective(objectiveData);
        }
        else
        {
            Debug.LogWarning($"No objective mapped for scene {sceneName}");
        }
    }

    private void OnDisable()
    {
        RemoveListeners();
    }
}
