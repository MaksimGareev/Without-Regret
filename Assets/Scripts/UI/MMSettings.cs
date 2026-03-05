using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem;

public class MMSettings : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction tabLeftAction;
    private InputAction tabRightAction;
    private InputAction resetSettingsAction;
    private InputAction applySettingsAction;
    private InputAction discardSettingsAction;

    [Header("Design Settings")]
    [Tooltip("Color for text when the setting has been changed but not yet applied.")]
    [SerializeField] private Color pendingChangeColor = Color.yellow;

    [Tooltip("Default color for all text in the settings menu.")]
    [SerializeField] private Color defaultColor = Color.white;

    [Tooltip("Color for the text of the control legends when they are enabled.")]
    [SerializeField] private Color legendsEnabledColor = Color.white;

    [Tooltip("Color for the text of the control legends when they are disabled.")]
    [SerializeField] private Color legendsDisabledColor = Color.gray;

    [Header("Parent Menu Reference")]
    [Tooltip("Reference to the parent menu (Main Menu or Pause Menu) to determine where to return after closing settings.")]
    [SerializeField] private GameObject parentMenu;

    [Header("Settings References")]
    // Video Settings
    [SerializeField] public TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    // Audio Settings
    [SerializeField] public Slider masterVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider dialogueVolumeSlider;
    // Controls Settings
    [SerializeField] public Slider mouseSensitivitySlider;
    [SerializeField] private Slider leftStickSensitivitySlider;
    [SerializeField] private Slider leftStickDeadZoneSlider;
    [SerializeField] private Slider rightStickSensitivitySlider;
    [SerializeField] private Slider rightStickDeadZoneSlider;

    [Header("Title Text References")]
    // Video
    [SerializeField] private TextMeshProUGUI resolutionTitleText;
    [SerializeField] private TextMeshProUGUI fullscreenTitleText;
    // Audio
    [SerializeField] private TextMeshProUGUI masterVolumeTitleText;
    [SerializeField] private TextMeshProUGUI SFXVolumeTitleText;
    [SerializeField] private TextMeshProUGUI musicVolumeTitleText;
    [SerializeField] private TextMeshProUGUI dialogueVolumeTitleText;
    // Controls
    [SerializeField] private TextMeshProUGUI mouseSensitivityTitleText;
    [SerializeField] private TextMeshProUGUI leftStickSensitivityTitleText;
    [SerializeField] private TextMeshProUGUI leftStickDeadZoneTitleText;
    [SerializeField] private TextMeshProUGUI rightStickSensitivityTitleText;
    [SerializeField] private TextMeshProUGUI rightStickDeadZoneTitleText;
    
    [Header("Value Text References")]
    // Audio
    [SerializeField] private TextMeshProUGUI masterVolumeValueText;
    [SerializeField] private TextMeshProUGUI SFXVolumeValueText;
    [SerializeField] private TextMeshProUGUI musicVolumeValueText;
    [SerializeField] private TextMeshProUGUI dialogueVolumeValueText;
    // Controls
    [SerializeField] private TextMeshProUGUI mouseSensitivityValueText;
    [SerializeField] private TextMeshProUGUI leftStickSensitivityValueText;
    [SerializeField] private TextMeshProUGUI leftStickDeadZoneValueText;
    [SerializeField] private TextMeshProUGUI rightStickSensitivityValueText;
    [SerializeField] private TextMeshProUGUI rightStickDeadZoneValueText;

    [Header("Buttons and UI Panels")]
    [SerializeField] public GameObject controllerLegends;
    [SerializeField] public GameObject keyboardLegends;
    [SerializeField] private Image resetButtonImage;
    [SerializeField] private Image applyButtonImage;
    [SerializeField] private Image discardButtonImage;
    [SerializeField] private Image resetKeyImage;
    [SerializeField] private Image applyKeyImage;
    [SerializeField] private Image discardKeyImage;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] public GameObject controlSchemeUI;
    [SerializeField] private GameObject videoSettingsUI;
    [SerializeField] private GameObject audioSettingsUI;
    [SerializeField] private GameObject controlsSettingsUI;
    [SerializeField] private GameObject confirmationPanel;

    [SerializeField] private Button controlSchemeUIButton;
    [SerializeField] private Button videoSettingsButton;
    [SerializeField] private Button audioSettingsButton;
    [SerializeField] private Button controlsSettingsButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button discardChangesButton;

    [HideInInspector] public bool controlSchemeOpen = false;
    [HideInInspector] public bool videoSettingsOpen = false;
    [HideInInspector] public bool audioSettingsOpen = false;
    [HideInInspector] public bool controlsSettingsOpen = false;

    // Temporary variables to hold settings before applying
    private int tempResolutionIndex;
    private bool tempIsFullscreen;
    private int tempMasterVolume;
    private int tempSFXVolume;
    private int tempMusicVolume;
    private int tempDialogueVolume;
    private int tempMouseSensitivity;
    private int tempLeftStickSensitivity;
    private int tempRightStickSensitivity;
    private int tempLeftStickDeadZone;
    private int tempRightStickDeadZone;

    // Default values for settings
    private int defaultResolutionIndex;
    private const int defaultIsFullscreen = 1;
    private const int defaultMasterVolume = 100;
    private const int defaultSFXVolume = 100;
    private const int defaultMusicVolume = 100;
    private const int defaultDialogueVolume = 100;
    private const int defaultMouseSensitivity = 100;
    private const int defaultLeftStickSensitivity = 100;
    private const int defaultRightStickSensitivity = 100;
    private const int defaultLeftStickDeadZone = 10;
    private const int defaultRightStickDeadZone = 10;

    private Resolution[] resolutions;

    public bool hasUnappliedChanges { get; private set; } = false;
    private bool hasChangedSettings = false;
    private ConfirmationUI confirmationUI;
    private ControlSchemeUI controlSchemeScript;

    private void Awake()
    {
        if (confirmationPanel == null)
        {
            Debug.LogError("confirmationPanel reference is missing.");
            return;
        }

        confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();

        if (confirmationUI == null)
        {
            Debug.LogError("ConfirmationUI component not found on confirmationPanel.");
        }

        controlSchemeScript = controlSchemeUI.GetComponentInChildren<ControlSchemeUI>();

        if (controlSchemeScript == null)
        {
            Debug.LogError("ControlSchemeUI component not found in any child of SettingsUI.");
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadResolutions();
        InitializeInputActions();
        SetUpListeners();
        LoadSettings();
        StartCoroutine(WaitToApplySettings());
    }

    private void InitializeInputActions()
    {
        // Initialize input actions
        tabLeftAction = inputActions.FindActionMap("UI").FindAction("TabLeft");
        if (tabLeftAction == null)
        {
            Debug.LogError("TabLeft action not found in InputActionAsset.");
            return;
        }

        tabRightAction = inputActions.FindActionMap("UI").FindAction("TabRight");
        if (tabRightAction == null)
        {
            Debug.LogError("TabRight action not found in InputActionAsset.");
            return;
        }

        resetSettingsAction = inputActions.FindActionMap("UI").FindAction("ResetSettings");
        if (resetSettingsAction == null)
        {
            Debug.LogError("ResetSettings action not found in InputActionAsset.");
            return;
        }

        applySettingsAction = inputActions.FindActionMap("UI").FindAction("ApplySettings");
        if (applySettingsAction == null)
        {
            Debug.LogError("ApplySettings action not found in InputActionAsset.");
            return;
        }

        discardSettingsAction = inputActions.FindActionMap("UI").FindAction("DiscardSettings");
        if (discardSettingsAction == null)
        {
            Debug.LogError("DiscardSettings action not found in InputActionAsset.");
            return;
        }
    }

    private void Update()
    {
        // Override button interactivity if confirmation panel is open
        if (confirmationPanel.activeSelf)
        {
            if (resetButton.interactable)
            {
                resetButton.interactable = false;
                resetKeyImage.color = legendsDisabledColor;
            }

            if (applyButton.interactable)
            {
                applyButton.interactable = false;
                applyKeyImage.color = legendsDisabledColor;
            }

            if (discardChangesButton.interactable)
            {
                discardChangesButton.interactable = false;
                discardKeyImage.color = legendsDisabledColor;
            }

            // Dont allow tabbing or other settings actions while confirming
            return;
        }

        if (tabLeftAction.triggered)
        {
            OnTabLeft();
        }

        if (tabRightAction.triggered)
        {
            OnTabRight();
        }

        if (resetSettingsAction.triggered)
        {
            OnResetSettings();
        }

        if (applySettingsAction.triggered)
        {
            OnApplySettings();
        }

        if (discardSettingsAction.triggered)
        {
            OnDiscardSettings();
        }

        // Set reset button interactable based on if applied settings are default
        if (hasChangedSettings && !resetButton.interactable)
        {
            resetButton.interactable = true;
            resetKeyImage.color = legendsEnabledColor;

        }
        else if (!hasChangedSettings && resetButton.interactable)
        {
            resetButton.interactable = false;
            resetKeyImage.color = legendsDisabledColor;
        }

        // Set apply and discard buttons interactable based on if there are unapplied changes
        // if (hasUnappliedChanges)
        // {
        //     if (!applyButton.interactable)
        //     {
        //         applyButton.interactable = true;
        //         applyKeyImage.color = legendsEnabledColor;
        //     }

        //     if (!discardChangesButton.interactable)
        //     {
        //         discardChangesButton.interactable = true;
        //         discardKeyImage.color = legendsEnabledColor;
        //     }
        // }
        // else if (!hasUnappliedChanges)
        // {
        //     if (applyButton.interactable)
        //     {
        //         applyButton.interactable = false;
        //         applyKeyImage.color = legendsDisabledColor;
        //     }
            
        //     if (discardChangesButton.interactable)
        //     {
        //         discardChangesButton.interactable = false;
        //         discardKeyImage.color = legendsDisabledColor;
        //     }
        // }
    }

    public void EnableSettingsPanel()
    {
        settingsPanel.SetActive(true);
        OpenVideoSettings();

        if (tabLeftAction != null)
        {
            tabLeftAction.Enable();
        }

        if (tabRightAction != null)
        {
            tabRightAction.Enable();
        }

        if (resetSettingsAction != null)
        {
            resetSettingsAction.Enable();
        }

        if (applySettingsAction != null)
        {
            applySettingsAction.Enable();
        }

        if (discardSettingsAction != null)
        {
            discardSettingsAction.Enable();
        }

        if (parentMenu != null)
        {
            if (parentMenu.GetComponent<MainMenu>() != null && parentMenu.GetComponent<MainMenu>().usingController)
            {
                controllerLegends.SetActive(true);
                keyboardLegends.SetActive(false);
            }
            else if (parentMenu.GetComponent<PauseManager>() != null && parentMenu.GetComponent<PauseManager>().usingController)
            {
                controllerLegends.SetActive(true);
                keyboardLegends.SetActive(false);
            }
            else
            {
                controllerLegends.SetActive(false);
                keyboardLegends.SetActive(true);
            }
        }

        SetUpListeners();
    }

    public void DisableSettingsPanel()
    {
        if (settingsPanel.activeSelf == false) return;

        settingsPanel.SetActive(false);

        if (tabLeftAction != null)
        {
            tabLeftAction.Disable();
        }

        if (tabRightAction != null)
        {
            tabRightAction.Disable();
        }

        if (resetSettingsAction != null)
        {
            resetSettingsAction.Disable();
        }

        if (applySettingsAction != null)
        {
            applySettingsAction.Disable();
        }

        if (discardSettingsAction != null)
        {
            discardSettingsAction.Disable();
        }

        if (parentMenu != null)
        {
            if (parentMenu.GetComponent<MainMenu>() != null && !parentMenu.GetComponent<MainMenu>().mainMenuPanel.activeSelf)
            {
                parentMenu.GetComponent<MainMenu>().OpenMainMenu();
            }
            else if (parentMenu.GetComponent<PauseManager>() != null && !parentMenu.GetComponent<PauseManager>().pauseMenuPanel.activeSelf)
            {
                parentMenu.GetComponent<PauseManager>().BackToPauseMenu();
            }
        }

        DisableListeners();
    }

    public void LoadResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        
        var uniqueResolutions = new System.Collections.Generic.List<Resolution>();
        foreach (var res in resolutions)
        {
            // Include 16:9, 16:10, and 4:3 aspect ratios.
            if (Mathf.Approximately((float)res.width / res.height, 16f / 9f) 
            || Mathf.Approximately((float)res.width / res.height, 16f / 10f) 
            || Mathf.Approximately((float)res.width / res.height, 4f / 3f))
            {
                // Avoid duplicates
                if (!uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                {
                    uniqueResolutions.Add(res);
                }
            }
        }

        resolutions = uniqueResolutions.ToArray();
        defaultResolutionIndex = resolutions.Length - 1; // Default to highest resolution
        
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Check if this resolution is the native resolution, set as current
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetUpListeners()
    {
        // Set up listeners for UI elements
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        dialogueVolumeSlider.onValueChanged.AddListener(SetDialogueVolume);
        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        leftStickSensitivitySlider.onValueChanged.AddListener(SetLeftStickSensitivity);
        rightStickSensitivitySlider.onValueChanged.AddListener(SetRightStickSensitivity);
        leftStickDeadZoneSlider.onValueChanged.AddListener(SetLeftStickDeadZone);
        rightStickDeadZoneSlider.onValueChanged.AddListener(SetRightStickDeadZone);
        
        // Set up button listeners
        controlSchemeUIButton.onClick.AddListener(OpenControlSchemeUI);
        videoSettingsButton.onClick.AddListener(OpenVideoSettings);
        audioSettingsButton.onClick.AddListener(OpenAudioSettings);
        controlsSettingsButton.onClick.AddListener(OpenControlsSettings);

        resetButton.onClick.AddListener(ConfirmBeforeReset);
        applyButton.onClick.AddListener(ConfirmBeforeApply);
        discardChangesButton.onClick.AddListener(ConfirmBeforeDiscardChanges);
    }

    private void OnTabRight()
    {
        if (!settingsPanel.activeSelf) return;
        
        if (!controlSchemeOpen && !confirmationPanel.activeSelf)
        {
            if (videoSettingsOpen)
            {
                OpenAudioSettings();
            }
            else if (audioSettingsOpen)
            {
                OpenControlsSettings();
            }
            else if (controlsSettingsOpen)
            {
                OpenVideoSettings();
            }
        }
        else if (controlSchemeOpen && !confirmationPanel.activeSelf)
        {
            controlSchemeScript.SwitchTabs();
        }
    }

    private void OnTabLeft()
    {
        if (!settingsPanel.activeSelf) return;

        if (!controlSchemeOpen && !confirmationPanel.activeSelf)
        {
            if (videoSettingsOpen)
            {
                OpenControlsSettings();
            }
            else if (audioSettingsOpen)
            {
                OpenVideoSettings();
            }
            else if (controlsSettingsOpen)
            {
                OpenAudioSettings();
            }
        }
        else if (controlSchemeOpen && !confirmationPanel.activeSelf)
        {
            controlSchemeScript.SwitchTabs();
        }
    }

    private void OnResetSettings()
    {
        if (!settingsPanel.activeSelf) return;

        if (!confirmationPanel.activeSelf && !controlSchemeOpen && hasChangedSettings)
        {
            ConfirmBeforeReset();
        }
    }

    private void OnApplySettings()
    {
        if (!settingsPanel.activeSelf) return;

        if (!confirmationPanel.activeSelf && !controlSchemeOpen && hasUnappliedChanges)
        {
            ConfirmBeforeApply();
        }
    }

    private void OnDiscardSettings()
    {
        if (!settingsPanel.activeSelf) return;

        if (!confirmationPanel.activeSelf && !controlSchemeOpen && hasUnappliedChanges)
        {
            ConfirmBeforeDiscardChanges();
        }
    }

    private void OpenVideoSettings()
    {
        // Set bools
        videoSettingsOpen = true;
        audioSettingsOpen = false;
        controlsSettingsOpen = false;

        // Set button text colors
        videoSettingsButton.gameObject.GetComponent<SelectableHighlighting>().stayHighlighted = true;
        videoSettingsButton.gameObject.GetComponent<SelectableHighlighting>().ApplyHighlight(true); // Highlight video settings button
        audioSettingsButton.gameObject.GetComponent<SelectableHighlighting>().RemoveHighlight(true);
        controlsSettingsButton.gameObject.GetComponent<SelectableHighlighting>().RemoveHighlight(true);

        EnableAllButtonsAndSliders();

        // Close other settings UIs, open video settings
        videoSettingsUI.SetActive(true);
        audioSettingsUI.SetActive(false);
        controlsSettingsUI.SetActive(false);
        
        // Set focus to resolution dropdown
        EventSystem.current.SetSelectedGameObject(resolutionDropdown.gameObject);
    }

    private void OpenAudioSettings()
    {
        // Set bools
        audioSettingsOpen = true;
        videoSettingsOpen = false;
        controlsSettingsOpen = false;

        // Set button text colors
        videoSettingsButton.gameObject.GetComponent<SelectableHighlighting>().RemoveHighlight(true);
        audioSettingsButton.gameObject.GetComponent<SelectableHighlighting>().stayHighlighted = true;
        audioSettingsButton.gameObject.GetComponent<SelectableHighlighting>().ApplyHighlight(true); // Highlight audio settings button
        controlsSettingsButton.gameObject.GetComponent<SelectableHighlighting>().RemoveHighlight(true);

        EnableAllButtonsAndSliders();

        // Close other settings UIs, open audio settings
        audioSettingsUI.SetActive(true);
        videoSettingsUI.SetActive(false);
        controlsSettingsUI.SetActive(false);

        // Set focus to first audio slider
        EventSystem.current.SetSelectedGameObject(masterVolumeSlider.gameObject);
    }

    private void OpenControlsSettings()
    {
        // Set bools
        controlsSettingsOpen = true;
        videoSettingsOpen = false;
        audioSettingsOpen = false;

        // Set button text colors
        videoSettingsButton.gameObject.GetComponent<SelectableHighlighting>().RemoveHighlight(true);
        audioSettingsButton.gameObject.GetComponent<SelectableHighlighting>().RemoveHighlight(true);
        controlsSettingsButton.gameObject.GetComponent<SelectableHighlighting>().stayHighlighted = true;
        controlsSettingsButton.gameObject.GetComponent<SelectableHighlighting>().ApplyHighlight(true); // Highlight controls settings button

        EnableAllButtonsAndSliders();

        // Close other settings UIs, open controls settings
        controlsSettingsUI.SetActive(true);
        videoSettingsUI.SetActive(false);
        audioSettingsUI.SetActive(false);

        // Set focus to first controls slider
        EventSystem.current.SetSelectedGameObject(mouseSensitivitySlider.gameObject);
    }

    private void SetResolution(int index)
    {
        // Set temporary values before actually applying
        tempResolutionIndex = index;

        CheckIfHasUnappliedSettings();
    }

    private void SetFullscreen(bool isFullscreen)
    {
        // Set temporary values before actually applying
        tempIsFullscreen = isFullscreen;

        CheckIfHasUnappliedSettings();
    }

    private void SetMasterVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetMasterVolume(Mathf.RoundToInt(volume));
        tempMasterVolume = Mathf.RoundToInt(volume);
        masterVolumeValueText.text = Mathf.RoundToInt(volume).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetSFXVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetSFXVolume(Mathf.RoundToInt(volume));
        tempSFXVolume = Mathf.RoundToInt(volume);
        SFXVolumeValueText.text = Mathf.RoundToInt(volume).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetMusicVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetMusicVolume(Mathf.RoundToInt(volume));
        tempMusicVolume = Mathf.RoundToInt(volume);
        musicVolumeValueText.text = Mathf.RoundToInt(volume).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetDialogueVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetDialogueVolume(Mathf.RoundToInt(volume));
        tempDialogueVolume = Mathf.RoundToInt(volume);
        dialogueVolumeValueText.text = Mathf.RoundToInt(volume).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetMouseSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying
        tempMouseSensitivity = Mathf.RoundToInt(sensitivity);
        mouseSensitivityValueText.text = Mathf.RoundToInt(sensitivity).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetLeftStickSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying
        tempLeftStickSensitivity =  Mathf.RoundToInt(sensitivity);
        leftStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetRightStickSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying    
        tempRightStickSensitivity =  Mathf.RoundToInt(sensitivity);
        rightStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetLeftStickDeadZone(float deadZone)
    {
        // Set temporary values before actually applying
        tempLeftStickDeadZone =  Mathf.RoundToInt(deadZone);
        leftStickDeadZoneValueText.text = Mathf.RoundToInt(deadZone).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void SetRightStickDeadZone(float deadZone)
    {
        // Set temporary values before actually applying
        tempRightStickDeadZone =  Mathf.RoundToInt(deadZone);
        rightStickDeadZoneValueText.text = Mathf.RoundToInt(deadZone).ToString("F0") + "%";

        CheckIfHasUnappliedSettings();
    }

    private void ConfirmBeforeApply()
    {
        confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();

        if (confirmationPanel == null)
        {
            Debug.LogError("confirmationPanel reference is missing.");
            return;
        }

        if (confirmationUI == null)
        {
            Debug.LogError("ConfirmationUI component not found on confirmationPanel.");
            return;
        }

        DisableAllButtonsAndSliders();
        tabLeftAction.Disable();
        tabRightAction.Disable();

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.ApplySettings, 
            () => 
            {
                ApplySettings();
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                tabLeftAction.Enable();
                tabRightAction.Enable();
                EventSystem.current.SetSelectedGameObject
                (
                    videoSettingsOpen? resolutionDropdown.gameObject :
                    audioSettingsOpen ? masterVolumeSlider.gameObject :
                    controlsSettingsOpen ? mouseSensitivitySlider.gameObject :
                    applyButton.gameObject
                );
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                tabLeftAction.Enable();
                tabRightAction.Enable();
                EventSystem.current.SetSelectedGameObject(applyButton.gameObject);
            });        
    }

    private void ConfirmBeforeReset()
    {
        confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();

        if (confirmationPanel == null)
        {
            Debug.LogError("confirmationPanel reference is missing.");
            return;
        }

        if (confirmationUI == null)
        {
            Debug.LogError("ConfirmationUI component not found on confirmationPanel.");
            return;
        }

        DisableAllButtonsAndSliders();
        tabLeftAction.Disable();
        tabRightAction.Disable();

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.ResetSettings, 
            () => 
            {
                ResetSettings();
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                tabLeftAction.Enable();
                tabRightAction.Enable();
                EventSystem.current.SetSelectedGameObject
                (
                    videoSettingsOpen? resolutionDropdown.gameObject :
                    audioSettingsOpen ? masterVolumeSlider.gameObject :
                    controlsSettingsOpen ? mouseSensitivitySlider.gameObject :
                    applyButton.gameObject
                );
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                tabLeftAction.Enable();
                tabRightAction.Enable();
                EventSystem.current.SetSelectedGameObject(resetButton.gameObject);
            });
    }

    private void ConfirmBeforeDiscardChanges()
    {
        confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();

        if (confirmationPanel == null)
        {
            Debug.LogError("confirmationPanel reference is missing.");
            return;
        }

        if (confirmationUI == null)
        {
            Debug.LogError("ConfirmationUI component not found on confirmationPanel.");
            return;
        }

        DisableAllButtonsAndSliders();
        tabLeftAction.Disable();
        tabRightAction.Disable();

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.DiscardChanges, 
            () => 
            {
                DiscardChanges();
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                tabLeftAction.Enable();
                tabRightAction.Enable();
                EventSystem.current.SetSelectedGameObject
                (
                    videoSettingsOpen? resolutionDropdown.gameObject :
                    audioSettingsOpen ? masterVolumeSlider.gameObject :
                    controlsSettingsOpen ? mouseSensitivitySlider.gameObject :
                    applyButton.gameObject
                );
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                tabLeftAction.Enable();
                tabRightAction.Enable();
                EventSystem.current.SetSelectedGameObject(discardChangesButton.gameObject);
            });
    }

    public void ConfirmBeforeLeaveWithoutApplying()
    {
        confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();

        if (confirmationPanel == null)
        {
            Debug.LogError("confirmationPanel reference is missing.");
            return;
        }

        if (confirmationUI == null)
        {
            Debug.LogError("ConfirmationUI component not found on confirmationPanel.");
            return;
        }

        DisableAllButtonsAndSliders();
        tabLeftAction.Disable();
        tabRightAction.Disable();

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.LeaveBeforeApplyingSettings, 
            () => 
            {
                // Logic to leave settings menu without applying changes
                DiscardChanges();
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                DisableSettingsPanel();
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
                tabLeftAction.Enable();
                tabRightAction.Enable();
                EventSystem.current.SetSelectedGameObject(applyButton.gameObject);
            });
    }

    private void ApplySettings()
    {
        // Apply video settings
        Resolution resolution = resolutions[tempResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, tempIsFullscreen);

        Screen.fullScreen = tempIsFullscreen;

        // Apply audio settings
        AudioManager.Instance.SetMasterVolume(tempMasterVolume);
        AudioManager.Instance.SetSFXVolume(tempSFXVolume);
        AudioManager.Instance.SetMusicVolume(tempMusicVolume);
        AudioManager.Instance.SetDialogueVolume(tempDialogueVolume);

        // Apply control settings
        GameSettings.ApplyControls(tempMouseSensitivity, tempLeftStickSensitivity, tempRightStickSensitivity, tempLeftStickDeadZone, tempRightStickDeadZone);
        
        // Save settings to PlayerPrefs
        PlayerPrefs.SetInt("resolution", tempResolutionIndex);
        PlayerPrefs.SetInt("fullscreen", tempIsFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("masterVolume", tempMasterVolume);
        PlayerPrefs.SetInt("SFXVolume", tempSFXVolume);
        PlayerPrefs.SetInt("musicVolume", tempMusicVolume);
        PlayerPrefs.SetInt("dialogueVolume", tempDialogueVolume);
        PlayerPrefs.SetInt("mouseSensitivity", tempMouseSensitivity);
        PlayerPrefs.SetInt("leftStickSensitivity", tempLeftStickSensitivity);
        PlayerPrefs.SetInt("rightStickSensitivity", tempRightStickSensitivity);
        PlayerPrefs.SetInt("leftStickDeadZone", tempLeftStickDeadZone);
        PlayerPrefs.SetInt("rightStickDeadZone", tempRightStickDeadZone);

        // Set texts back to white
        resolutionTitleText.color = defaultColor;
        fullscreenTitleText.color = defaultColor;
        masterVolumeTitleText.color = defaultColor;
        SFXVolumeTitleText.color = defaultColor;
        musicVolumeTitleText.color = defaultColor;
        dialogueVolumeTitleText.color = defaultColor;
        mouseSensitivityTitleText.color = defaultColor;
        leftStickSensitivityTitleText.color = defaultColor;
        rightStickSensitivityTitleText.color = defaultColor;
        leftStickDeadZoneTitleText.color = defaultColor;
        rightStickDeadZoneTitleText.color = defaultColor;

        masterVolumeValueText.color = defaultColor;
        SFXVolumeValueText.color = defaultColor;
        musicVolumeValueText.color = defaultColor;
        dialogueVolumeValueText.color = defaultColor;
        mouseSensitivityValueText.color = defaultColor;
        leftStickSensitivityValueText.color = defaultColor;
        rightStickSensitivityValueText.color = defaultColor;
        leftStickDeadZoneValueText.color = defaultColor;
        rightStickDeadZoneValueText.color = defaultColor;

        PlayerPrefs.Save();

        hasUnappliedChanges = false;

        CheckIfHasDefaultSettings();
    }

    private void ResetSettings()
    {
        // Clear PlayerPrefs to reset to default settings
        PlayerPrefs.DeleteKey("resolution");
        PlayerPrefs.DeleteKey("fullscreen");
        PlayerPrefs.DeleteKey("masterVolume");
        PlayerPrefs.DeleteKey("SFXVolume");
        PlayerPrefs.DeleteKey("musicVolume");
        PlayerPrefs.DeleteKey("dialogueVolume");
        PlayerPrefs.DeleteKey("mouseSensitivity");
        PlayerPrefs.DeleteKey("leftStickSensitivity");
        PlayerPrefs.DeleteKey("rightStickSensitivity");
        PlayerPrefs.DeleteKey("leftStickDeadZone");
        PlayerPrefs.DeleteKey("rightStickDeadZone");

        // Load default settings
        LoadSettings();
        hasUnappliedChanges = false;
        CheckIfHasDefaultSettings();
        ApplySettings();
    }

    private void DiscardChanges()
    {
        // Revert temporary values to last applied settings
        LoadSettings();
        hasUnappliedChanges = false;

        CheckIfHasDefaultSettings();
    }

    private void CheckIfHasDefaultSettings()
    {
        // Check if current settings differ from default settings, set hasChangedSettings accordingly
        if (PlayerPrefs.GetInt("resolution") != defaultResolutionIndex ||
            PlayerPrefs.GetInt("fullscreen") != defaultIsFullscreen ||
            PlayerPrefs.GetInt("masterVolume") != defaultMasterVolume ||
            PlayerPrefs.GetInt("SFXVolume") != defaultSFXVolume ||
            PlayerPrefs.GetInt("musicVolume") != defaultMusicVolume ||
            PlayerPrefs.GetInt("dialogueVolume") != defaultDialogueVolume ||
            PlayerPrefs.GetInt("mouseSensitivity") != defaultMouseSensitivity ||
            PlayerPrefs.GetInt("leftStickSensitivity") != defaultLeftStickSensitivity ||
            PlayerPrefs.GetInt("rightStickSensitivity") != defaultRightStickSensitivity ||
            PlayerPrefs.GetInt("leftStickDeadZone") != defaultLeftStickDeadZone ||
            PlayerPrefs.GetInt("rightStickDeadZone") != defaultRightStickDeadZone)
        {
            hasChangedSettings = true;
        }
        else
        {
            hasChangedSettings = false;
        }        
    }

    private void CheckIfHasUnappliedSettings()
    {
        // Check if temporary settings differ from currently applied settings
        bool resolutionChanged = tempResolutionIndex != PlayerPrefs.GetInt("resolution", resolutions.Length - 1);
        bool fullscreenChanged = tempIsFullscreen != (PlayerPrefs.GetInt("fullscreen", 1) == 1);
        bool masterVolumeChanged = tempMasterVolume != PlayerPrefs.GetInt("masterVolume", defaultMasterVolume);
        bool SFXVolumeChanged = tempSFXVolume != PlayerPrefs.GetInt("SFXVolume", defaultSFXVolume);
        bool musicVolumeChanged = tempMusicVolume != PlayerPrefs.GetInt("musicVolume", defaultMusicVolume);
        bool dialogueVolumeChanged = tempDialogueVolume != PlayerPrefs.GetInt("dialogueVolume", defaultDialogueVolume);
        bool mouseSensitivityChanged = tempMouseSensitivity != PlayerPrefs.GetInt("mouseSensitivity", defaultMouseSensitivity);
        bool leftStickSensitivityChanged = tempLeftStickSensitivity != PlayerPrefs.GetInt("leftStickSensitivity", defaultLeftStickSensitivity);
        bool rightStickSensitivityChanged = tempRightStickSensitivity != PlayerPrefs.GetInt("rightStickSensitivity", defaultRightStickSensitivity);
        bool leftStickDeadZoneChanged = tempLeftStickDeadZone != PlayerPrefs.GetInt("leftStickDeadZone", defaultLeftStickDeadZone);
        bool rightStickDeadZoneChanged = tempRightStickDeadZone != PlayerPrefs.GetInt("rightStickDeadZone", defaultRightStickDeadZone);

        // Update hasUnappliedChanges based on if any setting has changed
        hasUnappliedChanges = 
        resolutionChanged 
        || fullscreenChanged 
        || masterVolumeChanged 
        || SFXVolumeChanged 
        || musicVolumeChanged 
        || dialogueVolumeChanged 
        || mouseSensitivityChanged 
        || leftStickSensitivityChanged 
        || rightStickSensitivityChanged 
        || leftStickDeadZoneChanged 
        || rightStickDeadZoneChanged;

        if (hasUnappliedChanges)
        {
            applyButton.interactable = true;
            applyKeyImage.color = legendsEnabledColor;
            discardChangesButton.interactable = true;
            discardKeyImage.color = legendsEnabledColor;
        }
        else
        {
            applyButton.interactable = false;
            applyKeyImage.color = legendsDisabledColor;
            discardChangesButton.interactable = false;
            discardKeyImage.color = legendsDisabledColor;
        }

        // Set text colors to indicate which settings have unapplied changes
        if (resolutionChanged)
        {
            resolutionTitleText.color = pendingChangeColor;
        }
        else
        {
            resolutionTitleText.color = defaultColor;
        }

        if (fullscreenChanged)
        {
            fullscreenTitleText.color = pendingChangeColor;
        }
        else
        {
            fullscreenTitleText.color = defaultColor;
        }

        if (masterVolumeChanged)
        {
            masterVolumeTitleText.color = pendingChangeColor;
            masterVolumeValueText.color = pendingChangeColor;
        }
        else
        {
            masterVolumeTitleText.color = defaultColor;
            masterVolumeValueText.color = defaultColor;
        }

        if (SFXVolumeChanged)
        {
            SFXVolumeTitleText.color = pendingChangeColor;
            SFXVolumeValueText.color = pendingChangeColor;
        }
        else
        {
            SFXVolumeTitleText.color = defaultColor;
            SFXVolumeValueText.color = defaultColor;
        }

        if (musicVolumeChanged)
        {
            musicVolumeTitleText.color = pendingChangeColor;
            musicVolumeValueText.color = pendingChangeColor;
        }
        else
        {
            musicVolumeTitleText.color = defaultColor;
            musicVolumeValueText.color = defaultColor;
        }

        if (dialogueVolumeChanged)
        {
            dialogueVolumeTitleText.color = pendingChangeColor;
            dialogueVolumeValueText.color = pendingChangeColor;
        }
        else
        {
            dialogueVolumeTitleText.color = defaultColor;
            dialogueVolumeValueText.color = defaultColor;
        }

        if (mouseSensitivityChanged)
        {
            mouseSensitivityTitleText.color = pendingChangeColor;
            mouseSensitivityValueText.color = pendingChangeColor;
        }
        else
        {
            mouseSensitivityTitleText.color = defaultColor;
            mouseSensitivityValueText.color = defaultColor;
        }

        if (leftStickSensitivityChanged)
        {
            leftStickSensitivityTitleText.color = pendingChangeColor;
            leftStickSensitivityValueText.color = pendingChangeColor;
        }
        else
        {
            leftStickSensitivityTitleText.color = defaultColor;
            leftStickSensitivityValueText.color = defaultColor;
        }

        if (rightStickSensitivityChanged)
        {
            rightStickSensitivityTitleText.color = pendingChangeColor;
            rightStickSensitivityValueText.color = pendingChangeColor;
        }
        else
        {
            rightStickSensitivityTitleText.color = defaultColor;
            rightStickSensitivityValueText.color = defaultColor;
        }

        if (leftStickDeadZoneChanged)
        {
            leftStickDeadZoneTitleText.color = pendingChangeColor;
            leftStickDeadZoneValueText.color = pendingChangeColor;
        }
        else
        {
            leftStickDeadZoneTitleText.color = defaultColor;
            leftStickDeadZoneValueText.color = defaultColor;
        }

        if (rightStickDeadZoneChanged)
        {
            rightStickDeadZoneTitleText.color = pendingChangeColor;
            rightStickDeadZoneValueText.color = pendingChangeColor;
        }
        else
        {
            rightStickDeadZoneTitleText.color = defaultColor;
            rightStickDeadZoneValueText.color = defaultColor;
        }
    }

    public void LoadSettings()
    {
        // Load settings from PlayerPrefs or set to default values if not found
        tempResolutionIndex = PlayerPrefs.GetInt("resolution", defaultResolutionIndex);
        tempIsFullscreen = PlayerPrefs.GetInt("fullscreen", defaultIsFullscreen) == 1;
        tempMasterVolume = PlayerPrefs.GetInt("masterVolume", defaultMasterVolume);
        tempSFXVolume = PlayerPrefs.GetInt("SFXVolume", defaultSFXVolume);
        tempMusicVolume = PlayerPrefs.GetInt("musicVolume", defaultMusicVolume);
        tempDialogueVolume = PlayerPrefs.GetInt("dialogueVolume", defaultDialogueVolume);
        tempMouseSensitivity = PlayerPrefs.GetInt("mouseSensitivity", defaultMouseSensitivity);
        tempLeftStickSensitivity = PlayerPrefs.GetInt("leftStickSensitivity", defaultLeftStickSensitivity);
        tempRightStickSensitivity = PlayerPrefs.GetInt("rightStickSensitivity", defaultRightStickSensitivity);
        tempLeftStickDeadZone = PlayerPrefs.GetInt("leftStickDeadZone", defaultLeftStickDeadZone);
        tempRightStickDeadZone = PlayerPrefs.GetInt("rightStickDeadZone", defaultRightStickDeadZone);

        // Update UI elements to reflect loaded settings
        resolutionDropdown.value = tempResolutionIndex;
        fullscreenToggle.isOn = tempIsFullscreen;
        masterVolumeSlider.value = tempMasterVolume;
        SFXVolumeSlider.value = tempSFXVolume;
        musicVolumeSlider.value = tempMusicVolume;
        dialogueVolumeSlider.value = tempDialogueVolume;
        mouseSensitivitySlider.value = tempMouseSensitivity;
        leftStickSensitivitySlider.value = tempLeftStickSensitivity;
        rightStickSensitivitySlider.value = tempRightStickSensitivity;
        leftStickDeadZoneSlider.value = tempLeftStickDeadZone;
        rightStickDeadZoneSlider.value = tempRightStickDeadZone;
    }

    private IEnumerator WaitToApplySettings()
    {
        // Wait until AudioManager is initialized
        yield return new WaitUntil(() => AudioManager.Instance != null);
        ApplySettings();
    }

    private void OpenControlSchemeUI()
    {
        if (controlSchemeUI != null && settingsUI != null)
        {
            controlSchemeUI.SetActive(true);
            settingsUI.SetActive(false);
            controlSchemeOpen = true;
            if (GetComponentInParent<MainMenu>() != null)
            {
                EventSystem.current.SetSelectedGameObject(GetComponentInParent<MainMenu>().backButton.gameObject);
            }
            else if (GetComponentInParent<PauseManager>() != null)
            {
                EventSystem.current.SetSelectedGameObject(GetComponentInParent<PauseManager>().backButton.gameObject);
            }
        }
    }

    public void CloseControlSchemeUI()
    {
        if (controlSchemeUI != null && settingsUI != null)
        {
            controlSchemeUI.SetActive(false);
            settingsUI.SetActive(true);
            controlSchemeOpen = false;
            EventSystem.current.SetSelectedGameObject(mouseSensitivitySlider.gameObject);
        }
    }

    private void DisableAllButtonsAndSliders()
    {
        Debug.Log("Disabling all buttons and sliders.");
        videoSettingsButton.interactable = false;
        audioSettingsButton.interactable = false;
        controlsSettingsButton.interactable = false;

        resetButton.interactable = false;
        applyButton.interactable = false;
        discardChangesButton.interactable = false;

        resolutionDropdown.interactable = false;
        fullscreenToggle.interactable = false;

        masterVolumeSlider.interactable = false;
        SFXVolumeSlider.interactable = false;
        musicVolumeSlider.interactable = false;
        dialogueVolumeSlider.interactable = false;

        mouseSensitivitySlider.interactable = false;
        leftStickSensitivitySlider.interactable = false;
        rightStickSensitivitySlider.interactable = false;
        leftStickDeadZoneSlider.interactable = false;
        rightStickDeadZoneSlider.interactable = false;

        controlSchemeUIButton.interactable = false;
    }

    private void EnableAllButtonsAndSliders()
    {
        Debug.Log("Enabling all buttons and sliders.");
        videoSettingsButton.interactable = true;
        audioSettingsButton.interactable = true;
        controlsSettingsButton.interactable = true;

        resetButton.interactable = hasChangedSettings;
        applyButton.interactable = hasUnappliedChanges;
        discardChangesButton.interactable = hasUnappliedChanges;

        resolutionDropdown.interactable = videoSettingsOpen;
        fullscreenToggle.interactable = videoSettingsOpen;

        masterVolumeSlider.interactable = audioSettingsOpen;
        SFXVolumeSlider.interactable = audioSettingsOpen;
        musicVolumeSlider.interactable = audioSettingsOpen;
        dialogueVolumeSlider.interactable = audioSettingsOpen;

        mouseSensitivitySlider.interactable = controlsSettingsOpen;
        leftStickSensitivitySlider.interactable = controlsSettingsOpen;
        rightStickSensitivitySlider.interactable = controlsSettingsOpen;
        leftStickDeadZoneSlider.interactable = controlsSettingsOpen;
        rightStickDeadZoneSlider.interactable = controlsSettingsOpen;

        controlSchemeUIButton.interactable = controlsSettingsOpen;
    }

    private void DisableListeners()
    {
        tabLeftAction?.Disable();
        tabRightAction?.Disable();
        resetSettingsAction?.Disable();
        applySettingsAction?.Disable();
        discardSettingsAction?.Disable();

        resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
        fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);

        masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        SFXVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        dialogueVolumeSlider.onValueChanged.RemoveListener(SetDialogueVolume);

        mouseSensitivitySlider.onValueChanged.RemoveListener(SetMouseSensitivity);
        leftStickSensitivitySlider.onValueChanged.RemoveListener(SetLeftStickSensitivity);
        rightStickSensitivitySlider.onValueChanged.RemoveListener(SetRightStickSensitivity);
        leftStickDeadZoneSlider.onValueChanged.RemoveListener(SetLeftStickDeadZone);
        rightStickDeadZoneSlider.onValueChanged.RemoveListener(SetRightStickDeadZone);
        
        controlSchemeUIButton.onClick.RemoveListener(OpenControlSchemeUI);

        videoSettingsButton.onClick.RemoveListener(OpenVideoSettings);
        audioSettingsButton.onClick.RemoveListener(OpenAudioSettings);
        controlsSettingsButton.onClick.RemoveListener(OpenControlsSettings);

        resetButton.onClick.RemoveListener(ConfirmBeforeReset);
        applyButton.onClick.RemoveListener(ConfirmBeforeApply);
        discardChangesButton.onClick.RemoveListener(ConfirmBeforeDiscardChanges);
    }

    private void OnDisable()
    {
        DisableListeners();
    }

    private void OnDestroy()
    {
        DisableListeners();
    }
}