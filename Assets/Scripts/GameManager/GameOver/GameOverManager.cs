using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    private bool sceneLoadListenerRegistered = false;

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

        ResetGameOverState();
        
        if (!confirmationPanel)
        {
            Debug.LogWarning("Confirmation panel reference is missing in GameOverManager.");
        }

        if (confirmationPanel)
        {
            confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        }
        else
        {
            Debug.LogError("Confirmation panel reference is missing in GameOverManager.");
            confirmationUI = null;
        }
        
        if (!confirmationUI)
        {
            Debug.Log("Confirmation panel cannot be found  in GameOverManager.");
        }

        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
            sceneLoadListenerRegistered = true;
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
        TryRegisterSceneLoadedListener();
    }

    private void Start()
    {
        TryRegisterSceneLoadedListener();
    }

    private void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActionAsset reference is missing in GameOverManager.");
            return;
        }
        
        // Initialize input actions
        cancelAction = inputActions.FindActionMap("UI")?.FindAction("Cancel");
        if (cancelAction == null)
        {
            Debug.LogError("Cancel action not found in InputActionAsset.");
        }
    }

    private void EnableInputActions()
    {
        if (!inputActions) return;

        inputActions.FindActionMap("UI")?.Enable();
        inputActions.FindActionMap("Player")?.Disable();
        cancelAction?.Enable();
    }

    private void DisableInputActions()
    {
        if (!inputActions) return;

        inputActions.FindActionMap("UI")?.Disable();
        inputActions.FindActionMap("Player")?.Enable();
        cancelAction?.Disable();
    }

    private void TryRegisterSceneLoadedListener()
    {
        if (sceneLoadListenerRegistered) return;

        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
            sceneLoadListenerRegistered = true;
        }
    }

    private void ResetGameOverState()
    {
        isGameOver = false;
        usingController = false;
    }

    public void PrepareForGameOver()
    {
        var cameraMovement = FindFirstObjectByType<CameraMovement>();
        if (cameraMovement)
        {
            cameraMovement.SetCameraLocked(true);
        }

        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController)
        {
            playerController.SetCutsceneLocked(true);
            playerController.DisableInput();
        }
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
        if (cancelAction != null && cancelAction.triggered && isGameOverUIActive() && !(confirmationPanel && confirmationPanel.activeSelf))
        {
            ConfirmBeforeQuit();
        }

        if (Time.timeSinceLevelLoad < 0.1f && gameOverUI && gameOverUI.activeSelf && !IsGameOver)
        {
            DisableGameOverUI();
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
        ResetGameOverState();
        inputActions?.FindActionMap("Player")?.Enable();
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
        
        if (EventSystem.current && retryButton)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.firstSelectedGameObject = retryButton.gameObject;
            EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        }

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
            inputActions?.FindActionMap("Player")?.Disable();
        }
    }

    private void DisableGameOverUI()
    {
        ResetGameOverState();

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
        
        if (EventSystem.current)
        {
            EventSystem.current.firstSelectedGameObject = null;
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Unlock camera when game over UI is disabled
        CameraMovement cam = FindFirstObjectByType<CameraMovement>();
        if (cam != null)
        {
            cam.SetCameraLocked(false);
        }

        // // Re-enable player input when game over UI is disabled
        // PlayerController playerController = FindFirstObjectByType<PlayerController>();
        // if (playerController != null)
        // {
        //     playerController.EnableInput();
        // }
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
        EventSystem.current?.SetSelectedGameObject(null);

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
                if (quitButton && EventSystem.current)
                {
                    EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
                }
            });
    }

    private void Quit()
    {
        ResetGameOverState();
        
        if (gameOverUI)
        {
            gameOverUI.SetActive(false);
        }
        
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
        if (SceneLoadManager.Instance && sceneLoadListenerRegistered)
        {
            SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(OnSceneLoaded);
        }
        DisableInputActions();
        RemoveListeners();
    }
}