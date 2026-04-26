using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction cancelAction;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver; // Public getter for isGameOver

    [Header("Events")]
    [HideInInspector] public UnityEvent onGameOver;

    private bool usingController = false;
    private ConfirmationUI confirmationUI;

    public bool isGameOverUIActive() => gameOverUI && gameOverUI.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        if (gameOverUI)
        {
            gameOverUI.SetActive(false);
        }
        
        if (!confirmationPanel)
        {
            Debug.LogWarning("Confirmation panel reference is missing in GameOverManager.");
        }
        
        confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        
        if (!confirmationUI)
        {
            Debug.Log("Confirmation panel cannot be found  in GameOverManager.");
        }

        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
        }
        else
        {
            Debug.Log("SceneLoadManager instance not found, cannot subscribe to scene loaded event.");
        }
        
    }
    
    private void OnEnable()
    {
        InitializeInputActions();
        AddListeners();
    }

    private void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActionAsset reference is missing in GameOverManager.");
            return;
        }
        
        // Initialize input actions
        cancelAction = inputActions.FindActionMap("UI").FindAction("Cancel");
        if (cancelAction == null)
        {
            Debug.LogError("Cancel action not found in InputActionAsset.");
        }
    }

    private void EnableInputActions()
    {
        inputActions.FindActionMap("UI").Enable();
        inputActions.FindActionMap("Player").Disable();
        cancelAction?.Enable();
    }

    private void DisableInputActions()
    {
        inputActions.FindActionMap("UI").Disable();
        inputActions.FindActionMap("Player").Enable();
        cancelAction?.Disable();
    }

    private void AddListeners()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(Restart);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(ConfirmBeforeQuit);
        }
    }

    private void RemoveListeners()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(Restart);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(ConfirmBeforeQuit);
        }
    }

    private void Update()
    {
        if (cancelAction.triggered && isGameOverUIActive() && !confirmationPanel.activeSelf)
        {
            ConfirmBeforeQuit();
        }

        if (Time.timeSinceLevelLoad < 0.1f && gameOverUI.activeSelf && !IsGameOver)
        {
            DisableGameOverUI();
        }
    }

    private void CheckMouseInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        bool mouseKeysMoved = mouseDelta.sqrMagnitude > 0.1f || Keyboard.current.anyKey.isPressed;

        if (!mouseKeysMoved) return;
        
        if (usingController)
        {
            usingController = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (EventSystem.current.currentSelectedGameObject)
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

        bool controllerMoved = 
            Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f 
            || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f
            || Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.1f;
        
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
                if (inputModule)
                {
                    inputModule.enabled = false;
                    inputModule.enabled = true;
                }
            }

            // If nothing is selected, set a default based on the active panel
            if (!es.currentSelectedGameObject)
            {
                if (confirmationPanel.activeSelf)
                {
                    es.SetSelectedGameObject(confirmationUI.cancelButton.gameObject);
                }
                else if (gameOverUI.activeSelf)
                {
                    es.SetSelectedGameObject(retryButton.gameObject);
                }
            }
        } 
    }

    private void EnableOtherCanvases()
    {
        // Debug.Log("Enabling other canvases from GameOverManager");
        if (!GameManager.Instance || SceneManager.GetActiveScene().name == "MainMenu") return;

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

        if (GameManager.Instance.pauseMenu && !GameManager.Instance.pauseMenu.activeSelf)
        {
            GameManager.Instance.pauseMenu.SetActive(PauseManager.Instance.isGamePaused);
        }

        if (GameManager.Instance.journalUI && !GameManager.Instance.journalUI.activeSelf)
        {
            GameManager.Instance.journalUI.SetActive(Journal.Instance.IsJournalOpen);
        }

        if (GameManager.Instance.objectivePanel && !GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(GameManager.Instance.objectiveCanvas.IsVisible());
        }
    }

    private void DisableOtherCanvases()
    {
        // Debug.Log("Disabling other canvases from GameOverManager");
        if (!GameManager.Instance || SceneManager.GetActiveScene().name == "MainMenu") return;

        if (GameManager.Instance.mainCanvas && GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(false);
        }

        if (GameManager.Instance.interactionIconsCanvas && GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(false);
        }

        if (GameManager.Instance.playerUICanvas && GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(false);
        }

        if (GameManager.Instance.pauseMenu && GameManager.Instance.pauseMenu.activeSelf)
        {
            GameManager.Instance.pauseMenu.SetActive(false);
        }

        if (GameManager.Instance.journalUI && GameManager.Instance.journalUI.activeSelf)
        {
            GameManager.Instance.journalUI.SetActive(false);
        }

        if (GameManager.Instance.objectivePanel && GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(false);
        }
    }

    private void OnSceneLoaded()
    {
        isGameOver = false;
        DisableGameOverUI();
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;

        Debug.Log("Game Over Triggered");
        
        onGameOver?.Invoke();

        Time.timeScale = 0f; // Pause the game

        EnableGameOverUI();
    }

    private void EnableGameOverUI()
    {
        if (!gameOverUI)
        {
            return;
        }
        
        EnableInputActions();
        EnableUIButtons();

        gameOverUI.SetActive(true);

        DisableOtherCanvases();

        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, gameOverUI);
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.firstSelectedGameObject = retryButton.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);

        // Lock camera when game over UI is active
        CameraMovement cam = FindFirstObjectByType<CameraMovement>();
        if (cam)
        {
            cam.SetCameraLocked(true);
        }

        // Disable player input when game over UI is active
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController)
        {
            playerController.DisableInput();
        }
    }

    private void DisableGameOverUI()
    {
        if (gameOverUI == null)
        {
            return;
        }
        
        DisableInputActions();

        gameOverUI.SetActive(false);

        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(false, null);
        }

        EnableOtherCanvases();

        EventSystem.current.firstSelectedGameObject = null;
        EventSystem.current.SetSelectedGameObject(null);

        // Unlock camera when game over UI is disabled
        CameraMovement cam = FindFirstObjectByType<CameraMovement>();
        if (cam != null)
        {
            cam.SetCameraLocked(false);
        }

        // Re-enable player input when game over UI is disabled
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.EnableInput();
        }
    }

    private void ConfirmBeforeQuit()
    {
        if (!confirmationPanel)
        {
            Quit();
            return;
        }

        confirmationPanel.SetActive(true);
        DisableUIButtons();
        EventSystem.current.SetSelectedGameObject(null);

        if (!confirmationUI)
        {
            Debug.Log("ConfirmationUI component not found on confirmation panel, quitting without confirmation.");
            Quit();
            return;
        }

        confirmationUI.ConfirmTask(ConfirmationType.QuitToMainMenu, 
            () => 
            {
                // Confirm action
                confirmationPanel.SetActive(false);
                EnableUIButtons();
                Quit();
            },
            () => 
            {
                // Cancel action
                confirmationPanel.SetActive(false);
                EnableUIButtons();
                EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
            });
    }

    private void Quit()
    {
        Time.timeScale = 1f; // Resume the game before quitting
        isGameOver = false;
        gameOverUI.SetActive(false);
        
        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            Debug.Log("SceneLoadManager instance not found, using SceneManager to load MainMenu.");
            SceneManager.LoadScene("MainMenu");
        }
        
    }

    private void Restart()
    {
        Time.timeScale = 1f; // Resume the game
        isGameOver = false;
        DisableGameOverUI();
        Debug.Log("Restarting scene: " + SceneManager.GetActiveScene().name);

        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("SceneLoadManager instance not found, using SceneManager to reload scene.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void DisableUIButtons()
    {
        if (retryButton)
        {
            retryButton.interactable = false;
        }

        if (quitButton)
        {
            quitButton.interactable = false;
        }
    }

    private void EnableUIButtons()
    {
        if (retryButton)
        {
            retryButton.interactable = true;
        }

        if (quitButton)
        {
            quitButton.interactable = true;
        }
    }

    private void OnDisable()
    {
        DisableInputActions();
        RemoveListeners();
    }

    private void OnDestroy()
    {
        if (SceneLoadManager.Instance != null) SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(OnSceneLoaded);
        DisableInputActions();
        RemoveListeners();
    }
}