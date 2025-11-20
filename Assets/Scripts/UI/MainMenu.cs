using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI versionNumberText;
    private string gameVersion = "v.0.0.1";
    private string firstLevelTitle = "Echo'sHouse";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OpenMainMenu();
        versionNumberText.text = gameVersion;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        loadGameButton.onClick.AddListener(OpenLoadGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(OpenMainMenu);
    }

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        
        backButton.gameObject.SetActive(false);
    }

    private void StartNewGame()
    {
        // Logic to start a new game
        SceneManager.LoadScene(firstLevelTitle);
        Debug.Log("Starting New Game...");
    }

    private void OpenLoadGame()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        
        backButton.gameObject.SetActive(true);
    }

    private void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        
        backButton.gameObject.SetActive(true);
    }

    private void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        
        backButton.gameObject.SetActive(true);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
