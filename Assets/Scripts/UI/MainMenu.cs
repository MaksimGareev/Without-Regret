using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction confirmAction;
    private InputAction cancelAction;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private MMSettings settingsScript;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject saveSlotsPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI versionNumberText;
    [SerializeField] private TextMeshProUGUI playButtonText;

    [Header("Music")]
    [SerializeField] private GameObject musicSource;

    private string gameVersion = "v.0.0.1";
    private SaveManager saveManager;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();

        UpdatePlayButton();
        OpenMainMenu();
        StartCoroutine(WaitToStartMusic());

        versionNumberText.text = gameVersion;
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Initialize input actions
        confirmAction = inputActions.FindActionMap("UI").FindAction("Submit");
        if (confirmAction == null)
        {
            Debug.LogError("Confirm action not found in InputActionAsset.");
            return;
        }
        confirmAction.Enable();

        cancelAction = inputActions.FindActionMap("UI").FindAction("Cancel");
        if (cancelAction == null)
        {
            Debug.LogError("Cancel action not found in InputActionAsset.");
            return;
        }
        cancelAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        HandleControllerCancelInput();
        DeleteSavesDebug(); // Debug shortcut to delete all saves and reload main menu
        CheckMouseInput();
        CheckControllerInput();
    }

    private void DeleteSavesDebug()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            for (int i = 1; i <= 3; i++)
            {
                SaveSystem.DeleteSave(i);
            }

            SceneManager.LoadScene("MainMenu");
        }
    }

    private void HandleControllerCancelInput()
    {
        if (cancelAction != null && cancelAction.triggered)
        {
            if (creditsPanel.activeSelf || saveSlotsPanel.activeSelf)
            {
                OpenMainMenu();
            }
            else if (settingsPanel.activeSelf && settingsScript != null && !confirmationPanel.activeSelf)
            {
                if (settingsScript.controlSchemeOpen)
                {
                    settingsScript.CloseControlSchemeUI();
                }
                else
                {
                    OpenMainMenu();
                }
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
                    EventSystem.current.SetSelectedGameObject(settingsScript.videoSettingsButton.gameObject);
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
        quitButton.onClick.AddListener(ConfirmBeforeQuit);
        backButton.onClick.AddListener(UIBackButton);
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

    private void UIBackButton()
    {
        if (settingsPanel.activeSelf && settingsScript != null && settingsScript.controlSchemeOpen)
        {
            settingsScript.CloseControlSchemeUI();
        }
        else
        {
            OpenMainMenu();
        }
    }

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        confirmationPanel.SetActive(false);

        UpdatePlayButton();
        
        backButton.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }

    private void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsScript.EnableSettingsPanel();
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        
        backButton.gameObject.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(settingsScript.videoSettingsButton.gameObject);
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

    private void ConfirmBeforeQuit()
    {
        confirmationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        confirmationUI.ConfirmTask(ConfirmationType.QuitToDesktop, QuitGame, OpenMainMenu);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private IEnumerator WaitToStartMusic()
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);
        musicSource.SetActive(true);
    }
}
