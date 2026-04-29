using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.InputSystem.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction confirmAction;
    private InputAction cancelAction;
    private InputAction TabRightAction;
    private InputAction TabLeftAction;

    [Header("UI Panels")]
    [SerializeField] public GameObject mainMenuPanel;
    [SerializeField] private SettingsMenu settingsScript;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] public GameObject saveSlotsPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] public Button backButton;
    [SerializeField] private Button feedbackSurveyButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI versionNumberText;
    [Tooltip("Game version number that will be applied to the version number text on the main menu.")]
    [SerializeField] private string gameVersion = "v.0.2.9";
    [SerializeField] private TextMeshProUGUI playButtonText;

    [Header("Music")]
    [Tooltip("Audio source for main menu music. Will start playing when the game manager instance is ready.")]
    [SerializeField] private GameObject musicSource;

    [Header("Cutscene")]
    [Tooltip("The cutscene that will play when the player starts a new game.")]
    [SerializeField] private CutsceneData introCutscene;

    // Feedback Survey URL
    private string feedbackSurveyURL = "https://docs.google.com/forms/d/e/1FAIpQLSe6KfbYdlWsa25Scm4URfYHRRS8lzQC3mZkm6tqyS_uxxHObA/viewform?usp=sharing&ouid=106294286738853521476";
    
    private SaveManager saveManager;
    //[HideInInspector] public bool usingController { get; private set; } = false;
    private Button lastSelectedButton;
    private float menuInputWarmupUntil;
    private const float MenuInputWarmupSeconds = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();

        UpdatePlayButton();
        lastSelectedButton = playButton;
        OpenMainMenu();
        StartCoroutine(WaitToStartMusic());

        versionNumberText.text = gameVersion;
        
        EventSystem.current.firstSelectedGameObject = playButton.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);

        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }

        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Initialize input actions
        if (inputActions && !inputActions.FindActionMap("UI").enabled)
        {
            inputActions.FindActionMap("UI").Enable();
        }
        
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

        menuInputWarmupUntil = Time.unscaledTime + MenuInputWarmupSeconds;
        StartCoroutine(EnsureMenuInputAfterLoad());
        EnsureMenuInputState();
    }
    
    private void OnEnable()
    {
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.OnInputModeChanged += OnInputModeChanged;
            Debug.Log("MainMenu subscribed to OnInputModeChanged");
            OnInputModeChanged(InputDeviceManager.Instance.CurrentMode);
        }
        
        playButton.onClick.AddListener(OpenSaveSlotsScreen);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(ConfirmBeforeQuit);
        backButton.onClick.AddListener(HandleUIBackButton);
        feedbackSurveyButton.onClick.AddListener(ConfirmBeforeFeedbackSurvey);
    }

    // Update is called once per frame
    void Update()
    {
        HandleControllerCancelInput();
        DeleteSavesDebug(); // Debug shortcut to delete all saves and reload main menu

        if (confirmationPanel.activeSelf && backButton.gameObject.activeSelf)
        {
            backButton.gameObject.SetActive(false);
        }
        else if (!confirmationPanel.activeSelf && !mainMenuPanel.activeSelf && !backButton.gameObject.activeSelf)
        {
            backButton.gameObject.SetActive(true);
        }

        EnsureMenuInputState();

        if (Time.unscaledTime < menuInputWarmupUntil)
        {
            EnsureMenuSelection();
        }
    }

    private IEnumerator EnsureMenuInputAfterLoad()
    {
        // Re-assert menu input state for a few frames in case another transition script toggles maps late.
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            EnsureMenuInputState();
            EnsureMenuSelection();
        }
    }

    private void EnsureMenuInputState()
    {
        InputActionMap uiMap = inputActions.FindActionMap("UI");
        if (uiMap != null && !uiMap.enabled)
        {
            uiMap.Enable();
        }

        InputActionMap playerMap = inputActions.FindActionMap("Player");
        if (playerMap != null && playerMap.enabled)
        {
            playerMap.Disable();
        }

        if (EventSystem.current && EventSystem.current.currentInputModule is InputSystemUIInputModule inputModule && !inputModule.enabled)
        {
            inputModule.enabled = true;
        }

        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, mainMenuPanel);
        }
    }

    private void EnsureMenuSelection()
    {
        if (!mainMenuPanel.activeSelf || confirmationPanel.activeSelf)
        {
            return;
        }

        EventSystem eventSystem = EventSystem.current;
        if (!eventSystem || eventSystem.currentSelectedGameObject)
        {
            return;
        }

        Button buttonToSelect = lastSelectedButton ? lastSelectedButton : playButton;
        eventSystem.firstSelectedGameObject = buttonToSelect.gameObject;
        eventSystem.SetSelectedGameObject(buttonToSelect.gameObject);
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
            if ((creditsPanel.activeSelf || saveSlotsPanel.activeSelf) && !confirmationPanel.activeSelf)
            {
                OpenMainMenu();
            }
            else if (settingsPanel.activeSelf && settingsScript != null && !confirmationPanel.activeSelf)
            {
                if (settingsScript.controlSchemeOpen)
                {
                    settingsScript.CloseControlSchemeUI();
                }
                else if (settingsScript.hasUnappliedChanges)
                {
                    settingsScript.ConfirmBeforeLeaveWithoutApplying();
                }
                else
                {
                    OpenMainMenu();
                }
            }
        }
    }

    private void OnInputModeChanged(InputDeviceManager.InputMode mode)
    {
        //Debug.Log("Input mode changed to: " + mode);

        switch (mode)
        {
            case InputDeviceManager.InputMode.Controller:
                
                if (settingsPanel.activeSelf && settingsScript 
                    && !settingsScript.controllerLegends.activeSelf && settingsScript.keyboardLegends.activeSelf)
                {
                    settingsScript.controllerLegends.SetActive(true);
                    settingsScript.keyboardLegends.SetActive(false);
                }
                
                break;
            
            case InputDeviceManager.InputMode.KeyboardMouse:
                
                if (settingsPanel.activeSelf && settingsScript 
                    && settingsScript.controllerLegends.activeSelf && !settingsScript.keyboardLegends.activeSelf)
                {
                    settingsScript.controllerLegends.SetActive(false);
                    settingsScript.keyboardLegends.SetActive(true);
                }
                
                break;
        }
    }

    private void UpdatePlayButton()
    {
        if (saveManager.AnySavesExist())
        {
            playButtonText.text = "Continue";
        }
        else
        {
            playButtonText.text = "Play Game";
        }
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
                OpenMainMenu();
            }
        }
        else
        {
            OpenMainMenu();
        }
    }

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsScript.DisableSettingsPanel();
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        confirmationPanel.SetActive(false);

        UpdatePlayButton();
        
        backButton.gameObject.SetActive(false);

        EventSystem.current.firstSelectedGameObject = lastSelectedButton.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);

        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, mainMenuPanel);
        }
    }

    private void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsScript.EnableSettingsPanel();
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        
        backButton.gameObject.SetActive(true);

        EventSystem.current.firstSelectedGameObject = settingsScript.resolutionDropdown.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, settingsPanel);
        }

        lastSelectedButton = settingsButton;
    }

    private void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        saveSlotsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        
        backButton.gameObject.SetActive(true);
        
        EventSystem.current.firstSelectedGameObject = backButton.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, creditsPanel);
        }

        lastSelectedButton = creditsButton;
    }

    private void OpenSaveSlotsScreen()
    {
        mainMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);

        backButton.gameObject.SetActive(true);

        SelectSaveMenuButton();
        
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, saveSlotsPanel);
        }

        lastSelectedButton = playButton;
    }

    public void SelectSaveMenuButton()
    {
        Button buttonToSelect;
        
        var saveSlots = saveSlotsPanel.GetComponentInChildren<SaveSlotUI>();

        if (saveSlots.playButtons[0].gameObject.activeSelf)
        {
            buttonToSelect = saveSlots.playButtons[0];
        }
        else if (saveSlots.deleteButtons[0].gameObject.activeSelf)
        {
            buttonToSelect = saveSlots.deleteButtons[0];
        }
        else if (saveSlots.newGameButtons[0].gameObject.activeSelf)
        {
            buttonToSelect = saveSlots.newGameButtons[0];
        }
        else
        {
            Debug.LogWarning("No active play or new game buttons found in save slots panel.");
            return;
        }
        
        EventSystem.current.firstSelectedGameObject = buttonToSelect.gameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        
        buttonToSelect.GetComponent<SelectableHighlighting>().ApplyHighlight();
    }

    private void ConfirmBeforeQuit()
    {
        confirmationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, confirmationPanel);
        }

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        confirmationUI.ConfirmTask(ConfirmationType.QuitToDesktop, QuitGame, OpenMainMenu);

        lastSelectedButton = quitButton;
    }

    private void ConfirmBeforeFeedbackSurvey()
    {
        confirmationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, confirmationPanel);
        }

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        confirmationUI.ConfirmTask(ConfirmationType.FeedbackSurvey, OpenFeedbackSurvey, OpenMainMenu);

        lastSelectedButton = feedbackSurveyButton;
    }

    private void OpenFeedbackSurvey()
    {
        if (!string.IsNullOrEmpty(feedbackSurveyURL))
        {
            Application.OpenURL(feedbackSurveyURL);
            Debug.Log("Opening Feedback Survey URL: " + feedbackSurveyURL);
        }
        else
        {
            Debug.LogWarning("Feedback survey URL is not set.");
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private IEnumerator WaitToStartMusic()
    {
        yield return new WaitUntil(() => AudioManager.Instance);
        musicSource.SetActive(true);
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(OpenSaveSlotsScreen);
        settingsButton.onClick.RemoveListener(OpenSettings);
        creditsButton.onClick.RemoveListener(OpenCredits);
        quitButton.onClick.RemoveListener(ConfirmBeforeQuit);
        backButton.onClick.RemoveListener(HandleUIBackButton);
        feedbackSurveyButton.onClick.RemoveListener(ConfirmBeforeFeedbackSurvey);
        
        inputActions.FindActionMap("UI").Disable();
        TabLeftAction?.Disable();
        TabRightAction?.Disable();
        confirmAction?.Disable();
        cancelAction?.Disable();
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(OpenSaveSlotsScreen);
        settingsButton.onClick.RemoveListener(OpenSettings);
        creditsButton.onClick.RemoveListener(OpenCredits);
        quitButton.onClick.RemoveListener(ConfirmBeforeQuit);
        backButton.onClick.RemoveListener(HandleUIBackButton);
        feedbackSurveyButton.onClick.RemoveListener(ConfirmBeforeFeedbackSurvey);
        
        inputActions.FindActionMap("UI").Disable();
        TabLeftAction?.Disable();
        TabRightAction?.Disable();
        confirmAction?.Disable();
        cancelAction?.Disable();

        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.OnInputModeChanged -= OnInputModeChanged;
            InputDeviceManager.Instance.SetUIActive(false, null);
        }
    }
    
    public CutsceneData GetIntroCutscene()
    {
        return introCutscene;
    }
}
