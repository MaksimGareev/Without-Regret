using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Singleton")]
    public static PauseManager Instance { get; private set; }

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction playerPauseAction;
    private InputAction UIPauseAction;
    private InputAction cancelAction;
    private InputAction TabRightAction;
    private InputAction TabLeftAction;

    [Header("UI Button References")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button reloadSaveButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] public Button backButton;

    [Header("UI Panels")]
    [SerializeField] public GameObject pauseMenuPanel;
    [SerializeField] private SettingsMenu settingsScript;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmationPanel;

    [HideInInspector] public bool isGamePaused = false;
    //[HideInInspector] public bool usingController { get; private set; } = false;
    private bool inventoryWasOpen = false;
    private bool awaitingMainMenuLoad;

    private void Awake()
    {
        // Make this a singleton
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

        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        // Initialize input actions
        playerPauseAction = inputActions.FindAction("Player/Pause");
        playerPauseAction.Enable();

        UIPauseAction = inputActions.FindAction("UI/Pause");
        UIPauseAction.Enable();

        cancelAction = inputActions.FindAction("UI/Cancel");
        cancelAction.Enable();

        TabRightAction = inputActions.FindActionMap("UI").FindAction("TabRight");
        if (TabRightAction == null)
        {
            Debug.LogError("Tab Right action not found in InputActionAsset.");
            return;
        }
        TabRightAction.Enable();

        TabLeftAction = inputActions.FindActionMap("UI").FindAction("TabLeft");
        if (TabLeftAction == null)
        {
            Debug.LogError("Tab Left action not found in InputActionAsset.");
            return;
        }
        TabLeftAction.Enable();
    }

    private void OnEnable()
    {
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.OnInputModeChanged += OnInputModeChanged;
            Debug.Log("PauseManager subscribed to OnInputModeChanged");
            OnInputModeChanged(InputDeviceManager.Instance.CurrentMode);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        backButton.gameObject.SetActive(false);
        SetUpListeners();
    }

    // Update is called once per frame
    void Update()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        // Do not run pause-state recovery or pause input in menu scenes.
        if (activeSceneName == "MainMenu" || activeSceneName == "Credits")
        {
            return;
        }

        if (Time.timeSinceLevelLoad < 0.1f && pauseMenuPanel.activeSelf)
        {
            ResumeGame();
        }
        
        if ((playerPauseAction.triggered || UIPauseAction.triggered) 
        && !Journal.Instance.IsJournalOpen 
        && !NewDialogueManager.Instance.DialogueIsActive 
        && !confirmationPanel.activeSelf 
        && !(GameOverManager.Instance && (GameOverManager.Instance.IsGameOver || GameManager.Instance.qteIsActive || GameManager.Instance.objectiveDebugScript.DebugUIIsActive || GameManager.Instance.lockPickUIScript.IsActive))
        && !SceneLoadManager.Instance.IsLoading 
        && !InteractionTutorialUI.Instance.IsShowing 
        && !(Ending.Instance && Ending.Instance.finished)
        && !CutsceneManager.Instance.isCutscenePlaying)
        {
            if (!pauseMenuPanel.activeSelf && !settingsPanel.activeSelf)
            {
                PauseGame();
            }
            else if (!pauseMenuPanel.activeSelf && settingsPanel.activeSelf)
            {
                if (settingsScript != null && settingsScript.controlSchemeOpen)
                {
                    settingsScript.CloseControlSchemeUI();
                }
                else if (settingsScript != null && settingsScript.hasUnappliedChanges)
                {
                    settingsScript.ConfirmBeforeLeaveWithoutApplying();
                }
                else
                {
                    BackToPauseMenu();
                }
            }
            else if (pauseMenuPanel.activeSelf && !settingsPanel.activeSelf)
            {
                ResumeGame();
            }
        }

        HandleControllerCancelInput();

        // CheckMouseInput();
        // CheckControllerInput();
        
        // if (usingController && !EventSystem.current.currentSelectedGameObject)
        // {
        //     usingController = false;
        //     Cursor.visible = true;
        //     Cursor.lockState = CursorLockMode.None;
        // }
    }
    
    private void OnInputModeChanged(InputDeviceManager.InputMode mode)
    {
        //Debug.Log($"PauseManager InputModeChanged: {mode}");
        
        switch (mode)
        {
            case InputDeviceManager.InputMode.Controller:
                
                // Switch UI Legends
                if (settingsPanel.activeSelf && settingsScript 
                    && !settingsScript.controllerLegends.activeSelf && settingsScript.keyboardLegends.activeSelf)
                {
                    settingsScript.controllerLegends.SetActive(true);
                    settingsScript.keyboardLegends.SetActive(false);
                }
                
                break;
            
            case InputDeviceManager.InputMode.KeyboardMouse:
                
                // Switch UI Legends
                if (settingsPanel.activeSelf && settingsScript
                    && settingsScript.controllerLegends.activeSelf && !settingsScript.keyboardLegends.activeSelf)
                {
                    settingsScript.controllerLegends.SetActive(false);
                    settingsScript.keyboardLegends.SetActive(true);
                }
                
                break;
        }
    }

    private void HandleControllerCancelInput()
    {
        if (cancelAction.triggered)
        {
            if (settingsPanel.activeSelf && !confirmationPanel.activeSelf)
            {
                if (settingsScript != null && settingsScript.controlSchemeOpen)
                {
                    settingsScript.CloseControlSchemeUI();
                }
                else if (settingsScript != null && settingsScript.hasUnappliedChanges)
                {
                    settingsScript.ConfirmBeforeLeaveWithoutApplying();
                }
                else
                {
                    BackToPauseMenu();
                }
            }
            else if (pauseMenuPanel.activeSelf && !confirmationPanel.activeSelf)
            {
                ResumeGame();
            }
        }
    }

    // private void CheckMouseInput()
    // {
    //     if (Mouse.current == null)
    //     {
    //         return;
    //     }
    //
    //     Vector2 mouseDelta = Mouse.current.delta.ReadValue();
    //
    //     bool mouseKeysMoved = mouseDelta.sqrMagnitude > 0.1f || Keyboard.current.anyKey.isPressed;
    //
    //     if (!mouseKeysMoved) return;
    //     
    //     if (usingController)
    //     {
    //         usingController = false;
    //         Cursor.visible = true;
    //         Cursor.lockState = CursorLockMode.None;
    //
    //         if (EventSystem.current.currentSelectedGameObject != null)
    //         {
    //             EventSystem.current.SetSelectedGameObject(null);
    //         }
    //
    //         if (settingsPanel.activeSelf && settingsScript != null && settingsScript.controllerLegends.activeSelf && !settingsScript.keyboardLegends.activeSelf)
    //         {
    //             settingsScript.controllerLegends.SetActive(false);
    //             settingsScript.keyboardLegends.SetActive(true);
    //         }
    //     }
    // }
    //
    // private void CheckControllerInput()
    // {
    //     if (Gamepad.current == null)
    //     {
    //         return;
    //     }
    //
    //     bool controllerMoved = 
    //         Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f 
    //         || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f
    //         || ((Gamepad.current.leftShoulder.IsPressed() || Gamepad.current.rightShoulder.IsPressed()) && settingsPanel.activeSelf);
    //     
    //     if (!controllerMoved)
    //     {
    //         return;
    //     }
    //
    //     if (!usingController)
    //     {
    //         usingController = true;
    //         Cursor.visible = false;
    //         Cursor.lockState = CursorLockMode.Locked;
    //
    //         var es = EventSystem.current;
    //
    //         // Clear selected GameObject if mouse was hovering over something
    //         if (es.IsPointerOverGameObject())
    //         {
    //             var ped = new PointerEventData(es)
    //             {
    //                 position = new Vector2(-99999f, -99999f)
    //             };
    //
    //             es.RaycastAll(ped, new System.Collections.Generic.List<RaycastResult>());
    //             es.SetSelectedGameObject(null);
    //
    //             InputSystemUIInputModule inputModule = es.currentInputModule as InputSystemUIInputModule;
    //             if (inputModule != null)
    //             {
    //                 inputModule.enabled = false;
    //                 inputModule.enabled = true;
    //             }
    //         }
    //
    //         // If nothing is selected, set a default based on the active panel
    //         if (es.currentSelectedGameObject == null)
    //         {
    //             if (confirmationPanel.activeSelf)
    //             {
    //                 es.SetSelectedGameObject(confirmationPanel.GetComponent<ConfirmationUI>().cancelButton.gameObject);
    //             }
    //             else if (pauseMenuPanel.activeSelf)
    //             {
    //                 es.SetSelectedGameObject(resumeButton.gameObject);
    //             }
    //             else if (settingsPanel.activeSelf && !settingsScript.controlSchemeOpen)
    //             {
    //                 if (settingsScript.videoSettingsOpen)
    //                 {
    //                     es.SetSelectedGameObject(settingsScript.resolutionDropdown.gameObject);
    //                 }
    //                 else if (settingsScript.audioSettingsOpen)
    //                 {
    //                     es.SetSelectedGameObject(settingsScript.masterVolumeSlider.gameObject);
    //                 }
    //                 else if (settingsScript.controlsSettingsOpen)
    //                 {
    //                     es.SetSelectedGameObject(settingsScript.mouseSensitivitySlider.gameObject);
    //                 }
    //             }
    //             else if (settingsPanel.activeSelf && settingsScript.controlSchemeOpen)
    //             {
    //                 es.SetSelectedGameObject(backButton.gameObject);
    //             }
    //
    //             if (settingsPanel.activeSelf && !settingsScript.controllerLegends.activeSelf && settingsScript.keyboardLegends.activeSelf)
    //             {
    //                 settingsScript.controllerLegends.SetActive(true);
    //                 settingsScript.keyboardLegends.SetActive(false);
    //             }
    //         }
    //     } 
    // }

    private void PauseGame()
    {
        SetUpListeners();

        // Logic to pause the game
        pauseMenuPanel.SetActive(true);
        
        Time.timeScale = 0f;

        isGamePaused = true;

        // Lock camera when pausing
        CameraMovement cam = FindFirstObjectByType<CameraMovement>();
        if (cam != null)
        {
            cam.SetCameraLocked(true);
        }

        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.DisableInput();
        }

        inputActions.FindActionMap("UI").Enable();
        
        if (GameManager.Instance && GameManager.Instance.inventoryInteractingScript)
        {
            ToggleInventoryUI toggleInventoryUI = FindFirstObjectByType<ToggleInventoryUI>();
            if (toggleInventoryUI && toggleInventoryUI.isEnabled)
            {
                GameManager.Instance.inventoryInteractingScript.DisableInventoryInput();
                inventoryWasOpen = true;
            }
        }

        // Disable other canvases
        DisableOtherCanvases();

        // Set initial selected button
        EventSystem.current.firstSelectedGameObject = resumeButton.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        InputDeviceManager.Instance?.SetUIActive(true, pauseMenuPanel);
    }

    private void SetUpListeners()
    {
        // Assign button listeners
        resumeButton.onClick.AddListener(ResumeGame);
        reloadSaveButton.onClick.AddListener(ConfirmBeforeReload);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(ConfirmBeforeQuit);
        backButton.onClick.AddListener(HandleUIBackButton);
    }

    private void RemoveListeners()
    {
        // Remove button listeners
        resumeButton.onClick.RemoveListener(ResumeGame);
        reloadSaveButton.onClick.RemoveListener(ConfirmBeforeReload);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitButton.onClick.RemoveListener(ConfirmBeforeQuit);
        backButton.onClick.RemoveListener(HandleUIBackButton);
    }

    private void HandleUIBackButton()
    {
        if (settingsPanel.activeSelf && settingsScript != null)
        {
            if (settingsScript.hasUnappliedChanges)
            {
                settingsScript.ConfirmBeforeLeaveWithoutApplying();
            }
            else if (settingsScript.controlSchemeOpen)
            {
                settingsScript.CloseControlSchemeUI();
            }
            else
            {
                BackToPauseMenu();
            }
        }
        else
        {
            BackToPauseMenu();
        }
    }

    public void BackToPauseMenu()
    {
        settingsScript.DisableSettingsPanel();
        pauseMenuPanel.SetActive(true);
        backButton.gameObject.SetActive(false);

        EventSystem.current.firstSelectedGameObject = resumeButton.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        InputDeviceManager.Instance?.SetUIActive(true, pauseMenuPanel);
    }

    public void ResumeGame()
    {
        RemoveListeners();

        // Logic to resume the game
        pauseMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        backButton.gameObject.SetActive(false);

        isGamePaused = false;

        // Unlock camera when resuming
        CameraMovement cam = FindFirstObjectByType<CameraMovement>();
        if (cam != null)
        {
            cam.SetCameraLocked(false);
        }

        // Re-enable other canvases
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            EnableOtherCanvases();
        }

        Time.timeScale = 1f;

        inputActions.FindActionMap("UI").Disable();
        
        if (GameManager.Instance && GameManager.Instance.inventoryInteractingScript && inventoryWasOpen)
        {
            GameManager.Instance.inventoryInteractingScript.EnableInventoryInput();
            inventoryWasOpen = false;
        }
        
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.EnableInput();
        }
        
        EventSystem.current.firstSelectedGameObject = null;
        InputDeviceManager.Instance?.SetUIActive(false, null);
        
        //Debug.Log("Resuming Game...");
    }

    private void EnableOtherCanvases()
    {
        if (!GameManager.Instance) return;

        if (GameManager.Instance.mainCanvas && !GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(true);
        }

        if (GameManager.Instance.interactionIconsCanvas && !GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(true);
        }

        if (GameManager.Instance.playerUICanvas && !GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(true);
        }

        if (GameManager.Instance.gameOverCanvas && !GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(GameOverManager.Instance.IsGameOver);
        }

        if (GameManager.Instance.objectivePanel && !GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(GameManager.Instance.objectiveCanvas.IsVisible());
        }
        
        if (GameManager.Instance.qteCanvas && !GameManager.Instance.qteCanvas.activeSelf)
        {
            GameManager.Instance.qteCanvas.SetActive(true);
        }
        
        BossEnemyController boss = FindFirstObjectByType<BossEnemyController>();
        if (boss && boss.slidersContainer)
        {
            boss.slidersContainer.gameObject.SetActive(true);
        }
    }

    private void DisableOtherCanvases()
    {
        if (!GameManager.Instance) return;

        if (GameManager.Instance.mainCanvas && GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(false);
        }

        if (GameManager.Instance.interactionIconsCanvas && GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(false);
        }

        if (GameManager.Instance.journalUI && GameManager.Instance.journalUI.activeSelf)
        {
            GameManager.Instance.journalUI.SetActive(false);
        }

        if (GameManager.Instance.playerUICanvas && GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(false);
        }

        if (GameManager.Instance.gameOverCanvas && GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(false);
        }

        if (GameManager.Instance.dialoguePanel && GameManager.Instance.dialoguePanel.activeSelf)
        {
            GameManager.Instance.dialoguePanel.SetActive(false);
        }

        if (GameManager.Instance.objectivePanel && GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(false);
        }
        
        if (GameManager.Instance.qteCanvas && GameManager.Instance.qteCanvas.activeSelf)
        {
            GameManager.Instance.qteCanvas.SetActive(false);
        }
        
        BossEnemyController boss = FindFirstObjectByType<BossEnemyController>();
        if (boss && boss.slidersContainer)
        {
            boss.slidersContainer.gameObject.SetActive(false);
        }
    }

    private void ConfirmBeforeQuit()
    {
        confirmationPanel.SetActive(true);

        DisableAllButtons();

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        InputDeviceManager.Instance?.SetUIActive(true, confirmationPanel);
        confirmationUI.ConfirmTask(ConfirmationType.QuitToMainMenu, 
            () => 
            {
                QuitToMainMenu();
                confirmationPanel.SetActive(false);
                EnableAllButtons();
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtons();
                EventSystem.current.firstSelectedGameObject = quitButton.gameObject;
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            });
    }

    private void ConfirmBeforeReload()
    {
        confirmationPanel.SetActive(true);

        DisableAllButtons();

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        InputDeviceManager.Instance?.SetUIActive(true, confirmationPanel);
        confirmationUI.ConfirmTask(ConfirmationType.ReloadSave, 
            () => 
            {
                // Reload save if confirmed
                ReloadSave();
                confirmationPanel.SetActive(false);
                EnableAllButtons();
            },
            () => 
            {
                // Do nothing if canceled
                confirmationPanel.SetActive(false);
                EnableAllButtons();
                EventSystem.current.firstSelectedGameObject = reloadSaveButton.gameObject;
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            });
    }

    private void ReloadSave()
    {
        // Logic to reload the last save
        if (GameManager.Instance != null && GameManager.Instance.sceneLoadManager != null)
        {
            GameManager.Instance.sceneLoadManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogError("SceneLoadManager reference is missing in the GameManager. Reloading scene directly without fade transition.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        ResumeGame();
        //Debug.Log("Reloading Save...");
    }

    private void OpenSettings()
    {
        // Logic to open settings menu
        pauseMenuPanel.SetActive(false);
        settingsScript.EnableSettingsPanel();
        backButton.gameObject.SetActive(true);
        
        EventSystem.current.firstSelectedGameObject = settingsScript.resolutionDropdown.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        InputDeviceManager.Instance?.SetUIActive(true, settingsPanel);
        
        //Debug.Log("Opening Settings...");
    }

    private void QuitToMainMenu()
    {
        // Save game before quitting
        if (SaveManager.Instance)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        // Force-close pause UI
        pauseMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        settingsPanel.SetActive(false);
        confirmationPanel.SetActive(false);
        backButton.gameObject.SetActive(false);

        RemoveListeners();

        DisableOtherCanvases();
        
        Time.timeScale = 1f; // Ensure time scale is reset
        isGamePaused = false;
        inventoryWasOpen = false;
        
        if (EventSystem.current)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.firstSelectedGameObject = null;
        }

        // Keep menu navigation responsive during scene transition.
        inputActions.FindActionMap("Player")?.Disable();
        inputActions.FindActionMap("UI")?.Enable();

        if (!awaitingMainMenuLoad)
        {
            awaitingMainMenuLoad = true;
            SceneManager.sceneLoaded += HandleMainMenuLoaded;
        }

        // Logic to quit to main menu
        if (GameManager.Instance&& GameManager.Instance.sceneLoadManager)
        {
            GameManager.Instance.sceneLoadManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("SceneLoadManager reference is missing in the GameManager. Loading Main Menu scene directly without fade transition.");
            SceneManager.LoadScene("MainMenu");
        }
        
    }

    private void HandleMainMenuLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu")
        {
            return;
        }

        SceneManager.sceneLoaded -= HandleMainMenuLoaded;
        awaitingMainMenuLoad = false;

        // Enforce menu-safe input state in case another system changed maps during load.
        inputActions.FindActionMap("Player")?.Disable();
        inputActions.FindActionMap("UI")?.Enable();

        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
    }

    private void DisableAllButtons()
    {
        resumeButton.interactable = false;
        reloadSaveButton.interactable = false;
        settingsButton.interactable = false;
        quitButton.interactable = false;
    }

    private void EnableAllButtons()
    {
        resumeButton.interactable = true;
        reloadSaveButton.interactable = true;
        settingsButton.interactable = true;
        quitButton.interactable = true;
    }

    private void OnDisable()
    {
        RemoveListeners();

        if (awaitingMainMenuLoad)
        {
            SceneManager.sceneLoaded -= HandleMainMenuLoaded;
            awaitingMainMenuLoad = false;
        }
        
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.OnInputModeChanged -= OnInputModeChanged;
        }
    }
}
