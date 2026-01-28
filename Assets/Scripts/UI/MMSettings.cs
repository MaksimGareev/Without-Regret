using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class MMSettings : MonoBehaviour
{
    [Header("Design Settings")]
    [SerializeField] private Color pendingChangeColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    [Header("Settings References")]
    // Video Settings
    [SerializeField] private TMP_Dropdown resolutionDropdown;
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
    [SerializeField] public Button videoSettingsButton;
    [SerializeField] private Button audioSettingsButton;
    [SerializeField] private Button controlsSettingsButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button discardChangesButton;

    [HideInInspector] public bool controlSchemeOpen = false;

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

    private void Update()
    {
        if (hasChangedSettings && !resetButton.interactable)
        {
            resetButton.interactable = true;
        }
        else if (!hasChangedSettings && resetButton.interactable)
        {
            resetButton.interactable = false;
        }

        if (hasUnappliedChanges)
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
        else if (!hasUnappliedChanges)
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
    }

    private void OpenVideoSettings()
    {
        // Close other settings UIs, open video settings
        videoSettingsUI.SetActive(true);
        audioSettingsUI.SetActive(false);
        controlsSettingsUI.SetActive(false);
    }

    private void OpenAudioSettings()
    {
        // Close other settings UIs, open audio settings
        audioSettingsUI.SetActive(true);
        videoSettingsUI.SetActive(false);
        controlsSettingsUI.SetActive(false);
    }

    private void OpenControlsSettings()
    {
        // Close other settings UIs, open controls settings
        controlsSettingsUI.SetActive(true);
        videoSettingsUI.SetActive(false);
        audioSettingsUI.SetActive(false);
    }

    private void SetResolution(int index)
    {
        // Set temporary values before actually applying
        tempResolutionIndex = index;
        resolutionTitleText.color = pendingChangeColor;
        hasUnappliedChanges = true;
    }

    private void SetFullscreen(bool isFullscreen)
    {
        // Set temporary values before actually applying
        tempIsFullscreen = isFullscreen;
        fullscreenTitleText.color = pendingChangeColor;
        hasUnappliedChanges = true;
    }

    private void SetMasterVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetMasterVolume(volume);
        tempMasterVolume = volume;
        masterVolumeTitleText.color = pendingChangeColor;
        masterVolumeValueText.color = pendingChangeColor;
        masterVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetSFXVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetSFXVolume(volume);
        tempSFXVolume = volume;
        SFXVolumeTitleText.color = pendingChangeColor;
        SFXVolumeValueText.color = pendingChangeColor;
        SFXVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetMusicVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetMusicVolume(volume);
        tempMusicVolume = volume;
        musicVolumeTitleText.color = pendingChangeColor;
        musicVolumeValueText.color = pendingChangeColor;
        musicVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetDialogueVolume(float volume)
    {
        // Set temporary values before actually applying
        AudioManager.Instance?.SetDialogueVolume(volume);
        tempDialogueVolume = volume;
        dialogueVolumeTitleText.color = pendingChangeColor;
        dialogueVolumeValueText.color = pendingChangeColor;
        dialogueVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetMouseSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying
        tempMouseSensitivity = sensitivity;
        mouseSensitivityTitleText.color = pendingChangeColor;
        mouseSensitivityValueText.color = pendingChangeColor;
        mouseSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetLeftStickSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying
        tempLeftStickSensitivity = sensitivity;
        leftStickSensitivityTitleText.color = pendingChangeColor;
        leftStickSensitivityValueText.color = pendingChangeColor;
        leftStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetRightStickSensitivity(float sensitivity)
    {
        // Set temporary values before actually applying    
        tempRightStickSensitivity = sensitivity;
        rightStickSensitivityTitleText.color = pendingChangeColor;
        rightStickSensitivityValueText.color = pendingChangeColor;
        rightStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetLeftStickDeadZone(float deadZone)
    {
        // Set temporary values before actually applying
        tempLeftStickDeadZone = deadZone;
        leftStickDeadZoneTitleText.color = pendingChangeColor;
        leftStickDeadZoneValueText.color = pendingChangeColor;
        leftStickDeadZoneValueText.text = Mathf.RoundToInt(deadZone * 100).ToString("F0") + "%";
        hasUnappliedChanges = true;
    }

    private void SetRightStickDeadZone(float deadZone)
    {
        // Set temporary values before actually applying
        tempRightStickDeadZone = deadZone;
        rightStickDeadZoneTitleText.color = pendingChangeColor;
        rightStickDeadZoneValueText.color = pendingChangeColor;
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

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.ApplySettings, 
            () => 
            {
                ApplySettings();
                confirmationPanel.SetActive(false);
            },
            () => 
            {
                confirmationPanel.SetActive(false);
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

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.ResetSettings, 
            () => 
            {
                ResetSettings();
                confirmationPanel.SetActive(false);
            },
            () => 
            {
                confirmationPanel.SetActive(false);
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

        confirmationPanel.SetActive(true);

        confirmationUI.ConfirmTask(ConfirmationType.DiscardChanges, 
            () => 
            {
                DiscardChanges();
                confirmationPanel.SetActive(false);
            },
            () => 
            {
                confirmationPanel.SetActive(false);
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
            EventSystem.current.SetSelectedGameObject(controlsSettingsButton.gameObject);
        }
    }
}