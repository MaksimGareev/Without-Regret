using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject saveSlotsPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI versionNumberText;
    [SerializeField] private TextMeshProUGUI playButtonText;
    private string gameVersion = "v.0.0.1";
    private SaveManager saveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();

        UpdatePlayButton();
        OpenMainMenu();

        versionNumberText.text = gameVersion;
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame)
        {
            if (settingsPanel.activeSelf || creditsPanel.activeSelf || saveSlotsPanel.activeSelf)
            {
                OpenMainMenu();
            }
        }

        // Debug shortcut to delete all saves and reload main menu
        if (Input.GetKeyDown(KeyCode.F5))
        {
            for (int i = 1; i <= 3; i++)
            {
                SaveSystem.DeleteSave(i);
            }

            SceneManager.LoadScene("MainMenu");
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

        if (mouseDelta.sqrMagnitude > 0.1f)
        {
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

        if (controllerMoved)
        {
            Cursor.visible = false;

            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (mainMenuPanel.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
                else if (settingsPanel.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(settingsPanel.GetComponentInChildren<MMSettings>().resolutionDropdown.gameObject);
                }
                else if (creditsPanel.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                }
            }
        }
    }

    private void OnEnable()
    {
        playButton.onClick.AddListener(OpenSaveSlotsScreen);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(OpenMainMenu);
    }

    private void UpdatePlayButton()
    {
        if (saveManager.AnySavesExist())
        {
            playButtonText.text = "Continue";
        }
        else
        {
            playButtonText.text = "New Game";
        }
    }

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);

        UpdatePlayButton();
        
        backButton.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }

    private void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        
        backButton.gameObject.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(settingsPanel.GetComponentInChildren<MMSettings>().resolutionDropdown.gameObject);
    }

    private void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        
        backButton.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
    }

    private void OpenSaveSlotsScreen()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);

        backButton.gameObject.SetActive(true);

        SelectSaveMenuButton();
    }

    public void SelectSaveMenuButton()
    {
        EventSystem.current.SetSelectedGameObject(saveSlotsPanel.GetComponentInChildren<SaveSlotUI>().playButtons[0].gameObject.activeSelf 
        ? saveSlotsPanel.GetComponentInChildren<SaveSlotUI>().playButtons[0].gameObject
        : saveSlotsPanel.GetComponentInChildren<SaveSlotUI>().newGameButtons[0].gameObject);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
