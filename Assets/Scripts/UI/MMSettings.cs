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

    [Header("Design Settings")]
    [SerializeField] private Color pendingChangeColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color TabEnabledColor = Color.aquamarine;

    [Header("Settings References")]
    // Video Settings
    [SerializeField] public TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    // Audio Settings
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider dialogueVolumeSlider;
    // Controls Settings
    [SerializeField] private Slider mouseSensitivitySlider;
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
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject controlSchemeUI;
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
    private float tempMasterVolume;
    private float tempSFXVolume;
    private float tempMusicVolume;
    private float tempDialogueVolume;
    private float tempMouseSensitivity;
    private float tempLeftStickSensitivity;
    private float tempRightStickSensitivity;
    private float tempLeftStickDeadZone;
    private float tempRightStickDeadZone;

    private Resolution[] resolutions;

    private bool hasUnappliedChanges = false;
    private bool hasChangedSettings = false;
    private ConfirmationUI confirmationUI;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadResolutions();
        InitializeInputActions();
        SetUpEvents();
        LoadSettings();
        StartCoroutine(WaitToApplySettings());

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
        tabLeftAction.Enable();

        tabRightAction = inputActions.FindActionMap("UI").FindAction("TabRight");
        if (tabRightAction == null)
        {
            Debug.LogError("TabRight action not found in InputActionAsset.");
            return;
        }
        tabRightAction.Enable();
    }

    private void Update()
    {
        if (hasChangedSettings && !resetButton.interactable && !confirmationPanel.activeSelf)
        {
            resetButton.interactable = true;
        }
        else if (!hasChangedSettings && resetButton.interactable && !confirmationPanel.activeSelf)
        {
            resetButton.interactable = false;
        }

        if (hasUnappliedChanges && !confirmationPanel.activeSelf)
        {
            if (!applyButton.interactable)
            {
                applyButton.interactable = true;
            }

            if (!discardChangesButton.interactable)
            {
                discardChangesButton.interactable = true;
            }
        }
        else if (!hasUnappliedChanges && !confirmationPanel.activeSelf)
        {
            if (applyButton.interactable)
            {
                applyButton.interactable = false;
            }
            
            if (discardChangesButton.interactable)
            {
                discardChangesButton.interactable = false;
            }
        }
    }

    public void EnableSettingsPanel()
    {
        settingsPanel.SetActive(true);
        OpenVideoSettings();
    }

    public void LoadResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        
        var uniqueResolutions = new System.Collections.Generic.List<Resolution>();
        foreach (var res in resolutions)
        {
            // Only add 16:9 resolutions
            if (Mathf.Approximately((float)res.width / res.height, 16f / 9f))
            {
                // Avoid duplicates
                if (!uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                {
                    uniqueResolutions.Add(res);
                }
            }
        }

        resolutions = uniqueResolutions.ToArray();
        
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

    private void SetUpEvents()
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

        tabLeftAction.performed += ctx => 
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
        };

        tabRightAction.performed += ctx => 
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
        };
    }

    private void OpenVideoSettings()
    {
        // Set bools
        videoSettingsOpen = true;
        audioSettingsOpen = false;
        controlsSettingsOpen = false;

        // Set button text colors
        videoSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = TabEnabledColor;
        audioSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = defaultColor;
        controlsSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = defaultColor;

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
        audioSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = TabEnabledColor;
        videoSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = defaultColor;
        controlsSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = defaultColor;

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
        controlsSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = TabEnabledColor;
        videoSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = defaultColor;
        audioSettingsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = defaultColor;

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
        resolutionTitleText.color = tempResolutionIndex == PlayerPrefs.GetInt("resolution", resolutions.Length - 1) ? defaultColor : pendingChangeColor;
        hasUnappliedChanges = true;
    }

    private void SetFullscreen(bool isFullscreen)
    {
        // Set temporary values before actually applying
        tempIsFullscreen = isFullscreen;
        fullscreenTitleText.color = tempIsFullscreen == (PlayerPrefs.GetInt("fullscreen", 1) == 1) ? defaultColor : pendingChangeColor;
        hasUnappliedChanges = true;
    }

    private void SetMasterVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetMasterVolume(volume);
        tempMasterVolume = volume;
        masterVolumeTitleText.color = tempMasterVolume == PlayerPrefs.GetFloat("masterVolume", 1f) ? defaultColor : pendingChangeColor;
        masterVolumeValueText.color = tempMasterVolume == PlayerPrefs.GetFloat("masterVolume", 1f) ? defaultColor : pendingChangeColor;
        masterVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetSFXVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetSFXVolume(volume);
        tempSFXVolume = volume;
        SFXVolumeTitleText.color = tempSFXVolume == PlayerPrefs.GetFloat("SFXVolume", 1f) ? defaultColor : pendingChangeColor;
        SFXVolumeValueText.color = tempSFXVolume == PlayerPrefs.GetFloat("SFXVolume", 1f) ? defaultColor : pendingChangeColor;
        SFXVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetMusicVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetMusicVolume(volume);
        tempMusicVolume = volume;
        musicVolumeTitleText.color = tempMusicVolume == PlayerPrefs.GetFloat("musicVolume", 1f) ? defaultColor : pendingChangeColor;
        musicVolumeValueText.color = tempMusicVolume == PlayerPrefs.GetFloat("musicVolume", 1f) ? defaultColor : pendingChangeColor;
        musicVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetDialogueVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetDialogueVolume(volume);
        tempDialogueVolume = volume;
        dialogueVolumeTitleText.color = tempDialogueVolume == PlayerPrefs.GetFloat("dialogueVolume", 1f) ? defaultColor : pendingChangeColor;
        dialogueVolumeValueText.color = tempDialogueVolume == PlayerPrefs.GetFloat("dialogueVolume", 1f) ? defaultColor : pendingChangeColor;
        dialogueVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetMouseSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying
        tempMouseSensitivity = sensitivity;
        mouseSensitivityTitleText.color = tempMouseSensitivity == PlayerPrefs.GetFloat("mouseSensitivity", 1f) ? defaultColor : pendingChangeColor;
        mouseSensitivityValueText.color = tempMouseSensitivity == PlayerPrefs.GetFloat("mouseSensitivity", 1f) ? defaultColor : pendingChangeColor;
        mouseSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetLeftStickSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying
        tempLeftStickSensitivity = sensitivity;
        leftStickSensitivityTitleText.color = tempLeftStickSensitivity == PlayerPrefs.GetFloat("leftStickSensitivity", 1f) ? defaultColor : pendingChangeColor;
        leftStickSensitivityValueText.color = tempLeftStickSensitivity == PlayerPrefs.GetFloat("leftStickSensitivity", 1f) ? defaultColor : pendingChangeColor;
        leftStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetRightStickSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying    
        tempRightStickSensitivity = sensitivity;
        rightStickSensitivityTitleText.color = tempRightStickSensitivity == PlayerPrefs.GetFloat("rightStickSensitivity", 1f) ? defaultColor : pendingChangeColor;
        rightStickSensitivityValueText.color = tempRightStickSensitivity == PlayerPrefs.GetFloat("rightStickSensitivity", 1f) ? defaultColor : pendingChangeColor;
        rightStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetLeftStickDeadZone(float deadZone)
    {
        // Set temporary values before actually applying
        tempLeftStickDeadZone = deadZone;
        leftStickDeadZoneTitleText.color = tempLeftStickDeadZone == PlayerPrefs.GetFloat("leftStickDeadZone", 0.1f) ? defaultColor : pendingChangeColor;
        leftStickDeadZoneValueText.color = tempLeftStickDeadZone == PlayerPrefs.GetFloat("leftStickDeadZone", 0.1f) ? defaultColor : pendingChangeColor;
        leftStickDeadZoneValueText.text = Mathf.RoundToInt(deadZone * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetRightStickDeadZone(float deadZone)
    {
        // Set temporary values before actually applying
        tempRightStickDeadZone = deadZone;
        rightStickDeadZoneTitleText.color = tempRightStickDeadZone == PlayerPrefs.GetFloat("rightStickDeadZone", 0.1f) ? defaultColor : pendingChangeColor;
        rightStickDeadZoneValueText.color = tempRightStickDeadZone == PlayerPrefs.GetFloat("rightStickDeadZone", 0.1f) ? pendingChangeColor : defaultColor;
        rightStickDeadZoneValueText.text = Mathf.RoundToInt(deadZone * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
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

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.ApplySettings, 
            () => 
            {
                ApplySettings();
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
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

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.ResetSettings, 
            () => 
            {
                ResetSettings();
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
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

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.DiscardChanges, 
            () => 
            {
                DiscardChanges();
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
            },
            () => 
            {
                confirmationPanel.SetActive(false);
                EnableAllButtonsAndSliders();
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
        // PlayerController.mouseSensitivity = tempMouseSensitivity;
        // PlayerController.leftStickSensitivity = tempLeftStickSensitivity;
        // PlayerController.rightStickSensitivity = tempRightStickSensitivity;
        // PlayerController.leftStickDeadZone = tempLeftStickDeadZone;
        // PlayerController.rightStickDeadZone = tempRightStickDeadZone;
        
        // Save settings to PlayerPrefs
        PlayerPrefs.SetInt("resolution", tempResolutionIndex);
        PlayerPrefs.SetInt("fullscreen", tempIsFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat("masterVolume", tempMasterVolume);
        PlayerPrefs.SetFloat("SFXVolume", tempSFXVolume);
        PlayerPrefs.SetFloat("musicVolume", tempMusicVolume);
        PlayerPrefs.SetFloat("dialogueVolume", tempDialogueVolume);
        PlayerPrefs.SetFloat("mouseSensitivity", tempMouseSensitivity);
        PlayerPrefs.SetFloat("leftStickSensitivity", tempLeftStickSensitivity);
        PlayerPrefs.SetFloat("rightStickSensitivity", tempRightStickSensitivity);
        PlayerPrefs.SetFloat("leftStickDeadZone", tempLeftStickDeadZone);
        PlayerPrefs.SetFloat("rightStickDeadZone", tempRightStickDeadZone);

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
        hasChangedSettings = false;

        if (PlayerPrefs.GetInt("resolution") != resolutions.Length - 1 ||
            PlayerPrefs.GetInt("fullscreen") != 1 ||
            PlayerPrefs.GetFloat("masterVolume") != 1f ||
            PlayerPrefs.GetFloat("SFXVolume") != 1f ||
            PlayerPrefs.GetFloat("musicVolume") != 1f ||
            PlayerPrefs.GetFloat("dialogueVolume") != 1f ||
            PlayerPrefs.GetFloat("mouseSensitivity") != 1f ||
            PlayerPrefs.GetFloat("leftStickSensitivity") != 1f ||
            PlayerPrefs.GetFloat("rightStickSensitivity") != 1f ||
            PlayerPrefs.GetFloat("leftStickDeadZone") != 0.1f ||
            PlayerPrefs.GetFloat("rightStickDeadZone") != 0.1f)
        {
            hasChangedSettings = true;
        }
    }

    public void LoadSettings()
    {
        // Load settings from PlayerPrefs or set to default values if not found
        tempResolutionIndex = PlayerPrefs.GetInt("resolution", resolutions.Length - 1);
        tempIsFullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;
        tempMasterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
        tempSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        tempMusicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        tempDialogueVolume = PlayerPrefs.GetFloat("dialogueVolume", 1f);
        tempMouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1f);
        tempLeftStickSensitivity = PlayerPrefs.GetFloat("leftStickSensitivity", 1f);
        tempRightStickSensitivity = PlayerPrefs.GetFloat("rightStickSensitivity", 1f);
        tempLeftStickDeadZone = PlayerPrefs.GetFloat("leftStickDeadZone", 0.1f);
        tempRightStickDeadZone = PlayerPrefs.GetFloat("rightStickDeadZone", 0.1f);

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
            EventSystem.current.SetSelectedGameObject(controlSchemeUI.GetComponent<ControlSchemeUI>().ControllerButton.gameObject);
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

    private void OnDisable()
    {
        tabLeftAction.Disable();
        tabRightAction.Disable();
    }
}