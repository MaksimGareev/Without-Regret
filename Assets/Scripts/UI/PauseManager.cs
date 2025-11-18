using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private string pauseButton = "Xbox Start Button";
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("UI Button References")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button reloadSaveButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Canvas[] otherCanvasesToDisable;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        SetUpEvents();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(pauseButton) || Input.GetKeyDown(pauseKey) || Input.GetKeyDown(KeyCode.P))
        {
            if (!pauseMenuPanel.activeSelf && !settingsPanel.activeSelf)
            {
                PauseGame();
            }
            else if (settingsPanel.activeSelf && Input.GetKeyDown(pauseKey))
            {
                BackToPauseMenu();
            }
            else if (pauseMenuPanel.activeSelf)
            {
                ResumeGame();
            }
        }
    }

    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game time
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        foreach (var canvas in otherCanvasesToDisable)
        {
            canvas.enabled = false;
        }

        Debug.Log("Game Paused");
    }

    private void SetUpEvents()
    {
        // Assign button listeners
        resumeButton.onClick.AddListener(ResumeGame);
        reloadSaveButton.onClick.AddListener(ReloadSave);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitToMainMenu);
        backButton.onClick.AddListener(BackToPauseMenu);
    }

    private void BackToPauseMenu()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        backButton.gameObject.SetActive(false);
    }

    private void ResumeGame()
    {
        // Logic to resume the game
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        foreach (var canvas in otherCanvasesToDisable)
        {
            canvas.enabled = true;
            InventoryUIController inventoryCanvas = canvas.GetComponentInChildren<InventoryUIController>();
            if (inventoryCanvas != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        Time.timeScale = 1f; // Resume game time
        
        Debug.Log("Resuming Game...");
    }

    private void ReloadSave()
    {
        // Logic to reload the last save
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Reloading Save...");
    }

    private void OpenSettings()
    {
        // Logic to open settings menu

        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        backButton.gameObject.SetActive(true);
        
        Debug.Log("Opening Settings...");
    }

    private void QuitToMainMenu()
    {
        // Logic to quit to main menu
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Quitting to Main Menu...");
    }
}
