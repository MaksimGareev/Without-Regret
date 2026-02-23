using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    [Header("Game Over State")]
    public bool isGameOver = false;

    [Header("Events")]
    [HideInInspector] public UnityEvent onGameOver;

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

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(Restart);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(Quit);
        }
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
        if (gameOverUI == null)
        {
            return;
        }

        gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Quit()
    {
        Time.timeScale = 1f; // Resume the game before quitting
        isGameOver = false;
        gameOverUI.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    private void Restart()
    {
        Time.timeScale = 1f; // Resume the game
        isGameOver = false;
        gameOverUI.SetActive(false);

        if (SaveManager.Instance != null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            SaveManager.Instance.LoadGame(SaveSystem.activeSaveSlot);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}