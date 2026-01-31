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

    [Header("UI Panels")]
    [SerializeField] public GameObject mainMenuPanel;
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
    [SerializeField] private Button feedbackSurveyButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI versionNumberText;
    [SerializeField] private TextMeshProUGUI playButtonText;

    [Header("Music")]
    [SerializeField] private GameObject musicSource;

    // Feedback Survey URL
    private string feedbackSurveyURL = "https://docs.google.com/forms/d/e/1FAIpQLSe6KfbYdlWsa25Scm4URfYHRRS8lzQC3mZkm6tqyS_uxxHObA/viewform?usp=sharing&ouid=106294286738853521476";

    private string gameVersion = "v.0.0.1";
    private SaveManager saveManager;
    private bool usingController = false;
    private Button lastSelectedButton;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();

        UpdatePlayButton();
        lastSelectedButton = playButton;
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

        if (confirmationPanel.activeSelf && backButton.gameObject.activeSelf)
        {
            backButton.gameObject.SetActive(false);
        }
        else if (!confirmationPanel.activeSelf && !mainMenuPanel.activeSelf && !backButton.gameObject.activeSelf)
        {
            backButton.gameObject.SetActive(true);
        }
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

        // Check if the controller has moved either the left stick or dpad
        bool controllerMoved = Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f;
        
        if (!controllerMoved) return;

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

                InputSystemUIInputModule inputModule = es.currentInputModule as InputSystemUIInputModule;
                if (inputModule != null)
                {
                    inputModule.enabled = false;
                    inputModule.enabled = true;
                }

                es.SetSelectedGameObject(null);
            }

            // If nothing is selected, set a default based on the active panel
            if (es.currentSelectedGameObject == null)
            {
                if (mainMenuPanel.activeSelf)
                {
                    es.SetSelectedGameObject(playButton.gameObject);
                }
                else if (settingsPanel.activeSelf && settingsScript.videoSettingsOpen)
                {
                    es.SetSelectedGameObject(settingsScript.resolutionDropdown.gameObject);
                }
                else if (creditsPanel.activeSelf)
                {
                    es.SetSelectedGameObject(backButton.gameObject);
                }
                else if (saveSlotsPanel.activeSelf)
                {
                    SelectSaveMenuButton();
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
        backButton.onClick.AddListener(HandleUIBackButton);
        feedbackSurveyButton.onClick.AddListener(ConfirmBeforeFeedbackSurvey);
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

        EventSystem.current.SetSelectedGameObject(lastSelectedButton.gameObject);
    }

    private void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsScript.EnableSettingsPanel();
        creditsPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        
        backButton.gameObject.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(settingsScript.resolutionDropdown.gameObject);

        lastSelectedButton = settingsButton;
    }

    private void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsScript.DisableSettingsPanel();
        saveSlotsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        
        backButton.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(backButton.gameObject);

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

        lastSelectedButton = playButton;
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

        lastSelectedButton = quitButton;
    }

    private void ConfirmBeforeFeedbackSurvey()
    {
        confirmationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

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
        yield return new WaitUntil(() => AudioManager.Instance != null);
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
    }
}
