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

    public bool isGameOverUIActive() => gameOverUI != null && gameOverUI.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        if (confirmationPanel == null)
        {
            Debug.LogWarning("Confirmation panel reference is missing in GameOverManager.");
        }

        SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
    }
    
    private void OnEnable()
    {
        EnableInputActions();
        AddListeners();
    }

    private void EnableInputActions()
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
            return;
        }
    }

    private void DisableInputActions()
    {
        if (cancelAction != null) cancelAction.Disable();
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

        CheckMouseInput();
        CheckControllerInput();
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

        bool controllerMoved = 
            Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f 
            || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f;
        
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
                if (confirmationPanel.activeSelf)
                {
                    es.SetSelectedGameObject(confirmationPanel.GetComponent<ConfirmationUI>().cancelButton.gameObject);
                }
                else if (gameOverUI.activeSelf)
                {
                    es.SetSelectedGameObject(quitButton.gameObject);
                }
            }
        } 
    }

    private void EnableOtherCanvases()
    {
        Debug.Log("Enabling other canvases from GameOverManager");
        if (GameManager.Instance == null || SceneManager.GetActiveScene().name == "MainMenu") return;

        if (GameManager.Instance.mainCanvas != null && !GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(true);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && !GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(true);
        }

        if (GameManager.Instance.playerUICanvas != null && !GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(true);
        }

        if (GameManager.Instance.pauseMenu != null && !GameManager.Instance.pauseMenu.activeSelf)
        {
            GameManager.Instance.pauseMenu.SetActive(PauseManager.Instance.isGamePaused);
        }

        if (GameManager.Instance.journalUI != null && !GameManager.Instance.journalUI.activeSelf)
        {
            GameManager.Instance.journalUI.SetActive(Journal.Instance.isJournalOpen);
        }

        if (GameManager.Instance.objectivePanel != null && !GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(GameManager.Instance.objectiveCanvas.IsVisible());
        }
    }

    private void DisableOtherCanvases()
    {
        Debug.Log("Disabling other canvases from GameOverManager");
        if (GameManager.Instance == null || SceneManager.GetActiveScene().name == "MainMenu") return;

        if (GameManager.Instance.mainCanvas != null && GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(false);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(false);
        }

        if (GameManager.Instance.playerUICanvas != null && GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(false);
        }

        if (GameManager.Instance.pauseMenu != null && GameManager.Instance.pauseMenu.activeSelf)
        {
            GameManager.Instance.pauseMenu.SetActive(false);
        }

        if (GameManager.Instance.journalUI != null && GameManager.Instance.journalUI.activeSelf)
        {
            GameManager.Instance.journalUI.SetActive(false);
        }

        if (GameManager.Instance.objectivePanel != null && GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(false);
        }
    }

    private void OnSceneLoaded()
    {
        if (TimerRingUI.Instance != null && TimerRingUI.Instance.currentRingState != TimerRingUI.RingState.Empty)
        {
            isGameOver = false;
            DisableGameOverUI();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;

        Debug.Log("Game Over Triggered");
        
        onGameOver?.Invoke();

        //Time.timeScale = 0f; // Pause the game

        EnableGameOverUI();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
    }

    private void EnableGameOverUI()
    {
        if (gameOverUI == null)
        {
            return;
        }

        gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        DisableOtherCanvases();

        EventSystem.current.SetSelectedGameObject(retryButton.gameObject);

        // Lock camera when game over UI is active
        CameraMovement cam = FindFirstObjectByType<CameraMovement>();
        if (cam != null)
        {
            cam.SetCameraLocked(true);
        }

        // Disable player input when game over UI is active
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
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

        gameOverUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        EnableOtherCanvases();

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
        if (confirmationPanel == null)
        {
            Quit();
            return;
        }

        confirmationPanel.SetActive(true);
        DisableUIButtons();
        EventSystem.current.SetSelectedGameObject(null);

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
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
        //EnableOtherCanvases();
        SceneLoadManager.Instance.LoadScene("MainMenu");
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
        }
    }

    private void DisableUIButtons()
    {
        if (retryButton != null)
        {
            retryButton.interactable = false;
        }

        if (quitButton != null)
        {
            quitButton.interactable = false;
        }
    }

    private void EnableUIButtons()
    {
        if (retryButton != null)
        {
            retryButton.interactable = true;
        }

        if (quitButton != null)
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
        SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(OnSceneLoaded);
        DisableInputActions();
        RemoveListeners();
    }
}