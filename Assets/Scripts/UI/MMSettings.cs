using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MMSettings : MonoBehaviour
{
    [Header("Settings References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider leftStickSensitivitySlider;
    [SerializeField] private Slider leftStickDeadZoneSlider;
    [SerializeField] private Slider rightStickSensitivitySlider;
    [SerializeField] private Slider rightStickDeadZoneSlider;
    
    [Header("Text Value References")]
    [SerializeField] private TextMeshProUGUI masterVolumeValueText;
    [SerializeField] private TextMeshProUGUI SFXVolumeValueText;
    [SerializeField] private TextMeshProUGUI musicVolumeValueText;
    [SerializeField] private TextMeshProUGUI mouseSensitivityValueText;
    [SerializeField] private TextMeshProUGUI leftStickSensitivityValueText;
    [SerializeField] private TextMeshProUGUI leftStickDeadZoneValueText;
    [SerializeField] private TextMeshProUGUI rightStickSensitivityValueText;
    [SerializeField] private TextMeshProUGUI rightStickDeadZoneValueText;

    [Header("Buttons and UI Panels")]
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

    [HideInInspector] public bool controlSchemeOpen = false;

    // Temporary variables to hold settings before applying
    private int tempResolutionIndex;
    private bool tempIsFullscreen;
    private float tempMasterVolume;
    private float tempSFXVolume;
    private float tempMusicVolume;
    private float tempMouseSensitivity;
    private float tempLeftStickSensitivity;
    private float tempRightStickSensitivity;
    private float tempLeftStickDeadZone;
    private float tempRightStickDeadZone;

    private Resolution[] resolutions;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadResolutions();
        SetUpEvents();
        LoadSettings();
    }

    private void OnEnable()
    {
        OpenVideoSettings();
    }

    private void LoadResolutions()
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

    public void SetResolution(int index)
    {
        tempResolutionIndex = index;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        tempIsFullscreen = isFullscreen;
    }

    public void SetMasterVolume(float volume)
    {
        // Assuming an AudioManager exists to handle volume
        // AudioListener.volume = volume;
        tempMasterVolume = volume;
        masterVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
    }

    public void SetSFXVolume(float volume)
    {
        // Assuming an AudioManager exists to handle SFX volume
        // AudioManager.SetSFXVolume(volume);
        tempSFXVolume = volume;
        SFXVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
    }

    public void SetMusicVolume(float volume)
    {
        // Assuming an AudioManager exists to handle music volume
        // AudioManager.SetMusicVolume(volume);
        tempMusicVolume = volume;
        musicVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        // Assuming a PlayerController exists to handle mouse sensitivity
        // PlayerController.mouseSensitivity = sensitivity;
        tempMouseSensitivity = sensitivity;
        mouseSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
    }

    public void SetLeftStickSensitivity(float sensitivity)
    {
        // Assuming a PlayerController exists to handle left stick sensitivity
        // PlayerController.leftStickSensitivity = sensitivity;
        tempLeftStickSensitivity = sensitivity;
        leftStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
    }

    public void SetRightStickSensitivity(float sensitivity)
    {
        // Assuming a PlayerController exists to handle right stick sensitivity
        // PlayerController.rightStickSensitivity = sensitivity;
        tempRightStickSensitivity = sensitivity;
        rightStickSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
    }

    public void SetLeftStickDeadZone(float deadZone)
    {
        // Assuming a PlayerController exists to handle left stick dead zone
        // PlayerController.leftStickDeadZone = deadZone;
        tempLeftStickDeadZone = deadZone;
        leftStickDeadZoneValueText.text = Mathf.RoundToInt(deadZone * 100).ToString("F0") + "%";
    }

    public void SetRightStickDeadZone(float deadZone)
    {
        // Assuming a PlayerController exists to handle right stick dead zone
        // PlayerController.rightStickDeadZone = deadZone;
        tempRightStickDeadZone = deadZone;
        rightStickDeadZoneValueText.text = Mathf.RoundToInt(deadZone * 100).ToString("F0") + "%";
    }

    private void ConfirmBeforeApply()
    {
        confirmationPanel.SetActive(true);

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
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
        confirmationPanel.SetActive(true);

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
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

    public void ApplySettings()
    {
        Resolution resolution = resolutions[tempResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, tempIsFullscreen);

        Screen.fullScreen = tempIsFullscreen;

        // Assuming an AudioManager exists to handle volume
        // AudioListener.volume = tempMasterVolume;
        // AudioManager.SetSFXVolume(tempSFXVolume);
        // AudioManager.SetMusicVolume(tempMusicVolume);

        // Assuming a PlayerController exists to handle sensitivities and dead zone
        // PlayerController.mouseSensitivity = tempMouseSensitivity;
        // PlayerController.leftStickSensitivity = tempLeftStickSensitivity;
        // PlayerController.rightStickSensitivity = tempRightStickSensitivity;
        // PlayerController.leftStickDeadZone = tempLeftStickDeadZone;
        // PlayerController.rightStickDeadZone = tempRightStickDeadZone;
        
        PlayerPrefs.SetInt("resolution", tempResolutionIndex);
        PlayerPrefs.SetInt("fullscreen", tempIsFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat("masterVolume", tempMasterVolume);
        PlayerPrefs.SetFloat("SFXVolume", tempSFXVolume);
        PlayerPrefs.SetFloat("musicVolume", tempMusicVolume);
        PlayerPrefs.SetFloat("mouseSensitivity", tempMouseSensitivity);
        PlayerPrefs.SetFloat("leftStickSensitivity", tempLeftStickSensitivity);
        PlayerPrefs.SetFloat("rightStickSensitivity", tempRightStickSensitivity);
        PlayerPrefs.SetFloat("leftStickDeadZone", tempLeftStickDeadZone);
        PlayerPrefs.SetFloat("rightStickDeadZone", tempRightStickDeadZone);

        PlayerPrefs.Save();
    }

    public void ResetSettings()
    {
        PlayerPrefs.DeleteKey("resolution");
        PlayerPrefs.DeleteKey("fullscreen");
        PlayerPrefs.DeleteKey("masterVolume");
        PlayerPrefs.DeleteKey("SFXVolume");
        PlayerPrefs.DeleteKey("musicVolume");
        PlayerPrefs.DeleteKey("mouseSensitivity");
        PlayerPrefs.DeleteKey("leftStickSensitivity");
        PlayerPrefs.DeleteKey("rightStickSensitivity");
        PlayerPrefs.DeleteKey("leftStickDeadZone");
        PlayerPrefs.DeleteKey("rightStickDeadZone");
        LoadSettings();
    }

    private void LoadSettings()
    {
        tempResolutionIndex = PlayerPrefs.GetInt("resolution", resolutions.Length - 1);
        tempIsFullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;
        tempMasterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
        tempSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        tempMusicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        tempMouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1f);
        tempLeftStickSensitivity = PlayerPrefs.GetFloat("leftStickSensitivity", 1f);
        tempRightStickSensitivity = PlayerPrefs.GetFloat("rightStickSensitivity", 1f);
        tempLeftStickDeadZone = PlayerPrefs.GetFloat("leftStickDeadZone", 0.1f);
        tempRightStickDeadZone = PlayerPrefs.GetFloat("rightStickDeadZone", 0.1f);

        resolutionDropdown.value = tempResolutionIndex;
        fullscreenToggle.isOn = tempIsFullscreen;
        masterVolumeSlider.value = tempMasterVolume;
        SFXVolumeSlider.value = tempSFXVolume;
        musicVolumeSlider.value = tempMusicVolume;
        mouseSensitivitySlider.value = tempMouseSensitivity;
        leftStickSensitivitySlider.value = tempLeftStickSensitivity;
        rightStickSensitivitySlider.value = tempRightStickSensitivity;
        leftStickDeadZoneSlider.value = tempLeftStickDeadZone;
        rightStickDeadZoneSlider.value = tempRightStickDeadZone;
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