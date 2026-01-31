using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Singleton")]
    public static PauseManager Instance { get; private set; }

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction playerPauseAction;
    private InputAction UIPauseAction;
    private InputAction cancelAction;

    [Header("UI Button References")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button reloadSaveButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("UI Panels")]
    [SerializeField] public GameObject pauseMenuPanel;
    [SerializeField] private MMSettings settingsScript;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Canvas[] otherCanvasesToDisable;

    [HideInInspector] public bool isGamePaused = false;
    private bool usingController = false;

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
        }

        // Initialize input actions
        playerPauseAction = inputActions.FindAction("Player/Pause");
        playerPauseAction.Enable();

        UIPauseAction = inputActions.FindAction("UI/Pause");
        UIPauseAction.Enable();

        cancelAction = inputActions.FindAction("UI/Cancel");
        cancelAction.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        backButton.gameObject.SetActive(false);
        SetUpEvents();
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            return; // Do not allow pausing in the main menu
        }
        
        if ((playerPauseAction.triggered || UIPauseAction.triggered) && !Journal.Instance.isJournalOpen)
        {
            if (!pauseMenuPanel.activeSelf && !settingsPanel.activeSelf)
            {
                PauseGame();
            }
            else if (settingsPanel.activeSelf)
            {
                BackToPauseMenu();
            }
            else if (pauseMenuPanel.activeSelf)
            {
                ResumeGame();
            }
        }

        HandleControllerCancelInput();

        CheckMouseInput();
        CheckControllerInput();
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

    private void CheckMouseInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        if (mouseDelta.sqrMagnitude > 0.1f && usingController)
        {
            usingController = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private void CheckControllerInput()
    {
        if (Gamepad.current == null)
        {
            return;
        }

        bool controllerMoved = Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f;
        
        if (!controllerMoved)
        {
            return;
        }

        if (!usingController)
        {
            usingController = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            var es = EventSystem.current;

            // Clear selected GameObject if mouse was hovering over something
            if (es.IsPointerOverGameObject())
            {
                var ped = new PointerEventData(es)
                {
                    position = new Vector2(-99999f, -99999f)
                };

                es.RaycastAll(ped, new System.Collections.Generic.List<RaycastResult>());
                es.SetSelectedGameObject(null);

                InputSystemUIInputModule inputModule = es.currentInputModule as InputSystemUIInputModule;
                if (inputModule != null)
                {
                    inputModule.enabled = false;
                    inputModule.enabled = true;
                }
            }

            // If nothing is selected, set a default based on the active panel
            if (es.currentSelectedGameObject == null)
            {
                if (pauseMenuPanel.activeSelf)
                {
                    es.SetSelectedGameObject(resumeButton.gameObject);
                }
                else if (settingsPanel.activeSelf && settingsScript.videoSettingsOpen)
                {
                    es.SetSelectedGameObject(settingsScript.resolutionDropdown.gameObject);
                }
            }
        } 
    }

    private void PauseGame()
    {
        // Save game before pausing
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        // Logic to pause the game
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isGamePaused = true;
        inputActions.FindActionMap("Player").Disable();
        inputActions.FindActionMap("UI").Enable();

        // Disable other canvases
        foreach (var canvas in otherCanvasesToDisable)
        {
            if (canvas != null)
                canvas.enabled = false;
        }

        // Set initial selected button
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);

        //Debug.Log("Game Paused");
    }

    private void SetUpEvents()
    {
        // Assign button listeners
        resumeButton.onClick.AddListener(ResumeGame);
        reloadSaveButton.onClick.AddListener(ConfirmBeforeReload);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(ConfirmBeforeQuit);
        backButton.onClick.AddListener(HandleUIBackButton);
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

        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void ResumeGame()
    {
        // Logic to resume the game
        pauseMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        backButton.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isGamePaused = false;

        // Re-enable other canvases
        foreach (var canvas in otherCanvasesToDisable)
        {
            if (canvas == null) continue;

            canvas.enabled = true;
            
            InventoryUIController inventoryCanvas = canvas.GetComponentInChildren<InventoryUIController>();
            if (inventoryCanvas != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        Time.timeScale = 1f;

        inputActions.FindActionMap("UI").Disable();
        inputActions.FindActionMap("Player").Enable();
        
        //Debug.Log("Resuming Game...");
    }

    private void ConfirmBeforeQuit()
    {
        confirmationPanel.SetActive(true);

        DisableAllButtons();

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
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
                EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
            });
    }

    private void ConfirmBeforeReload()
    {
        confirmationPanel.SetActive(true);

        DisableAllButtons();

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
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
                // Do nothing if cancelled
                confirmationPanel.SetActive(false);
                EnableAllButtons();
                EventSystem.current.SetSelectedGameObject(reloadSaveButton.gameObject);
            });
    }

    private void ReloadSave()
    {
        // Logic to reload the last save
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ResumeGame();
        //Debug.Log("Reloading Save...");
    }

    private void OpenSettings()
    {
        // Logic to open settings menu
        pauseMenuPanel.SetActive(false);
        settingsScript.EnableSettingsPanel();
        backButton.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(settingsScript.resolutionDropdown.gameObject);
        
        //Debug.Log("Opening Settings...");
    }

    private void QuitToMainMenu()
    {
        // Save game before quitting
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        // Logic to quit to main menu
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f; // Ensure time scale is reset
        isGamePaused = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    
        inputActions.FindActionMap("UI").Disable();
        inputActions.FindActionMap("Player").Enable();
        
        //Debug.Log("Quitting to Main Menu...");
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
}
