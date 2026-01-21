using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MMSettings : MonoBehaviour
{
    [Header("Settings References")]
    [SerializeField] public TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider controllerSensitivitySlider;
    [SerializeField] private Slider controllerDeadZoneSlider;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button applyButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI masterVolumeValueText;
    [SerializeField] private TextMeshProUGUI SFXVolumeValueText;
    [SerializeField] private TextMeshProUGUI musicVolumeValueText;
    [SerializeField] private TextMeshProUGUI mouseSensitivityValueText;
    [SerializeField] private TextMeshProUGUI controllerSensitivityValueText;
    [SerializeField] private TextMeshProUGUI controllerDeadZoneValueText;

    [Header("UI References")]
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject controlSchemeUI;
    [SerializeField] private Button controlSchemeUIButton;
    [HideInInspector] public bool controlSchemeOpen = false;

    private int tempResolutionIndex;
    private bool tempIsFullscreen;
    private float tempMasterVolume;
    private float tempSFXVolume;
    private float tempMusicVolume;
    private float tempMouseSensitivity;
    private float tempControllerSensitivity;
    private float tempControllerDeadZone;

    private Resolution[] resolutions;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadResolutions();
        SetUpEvents();
        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        controllerSensitivitySlider.onValueChanged.AddListener(SetControllerSensitivity);
        controllerDeadZoneSlider.onValueChanged.AddListener(SetControllerDeadZone);
        
        resetButton.onClick.AddListener(ResetSettings);
        applyButton.onClick.AddListener(LoadSettings);

        controlSchemeUIButton.onClick.AddListener(OpenControlSchemeUI);
    }

    public void SetResolution(int index)
    {
        tempResolutionIndex = index;
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        tempIsFullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
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

    public void SetControllerSensitivity(float sensitivity)
    {
        // Assuming a PlayerController exists to handle controller sensitivity
        // PlayerController.controllerSensitivity = sensitivity;
        tempControllerSensitivity = sensitivity;
        controllerSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
    }

    public void SetControllerDeadZone(float deadZone)
    {
        // Assuming a PlayerController exists to handle controller dead zone
        // PlayerController.controllerDeadZone = deadZone;
        tempControllerDeadZone = deadZone;
        controllerDeadZoneValueText.text = Mathf.RoundToInt(deadZone * 100).ToString("F0") + "%";
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
        // PlayerController.controllerSensitivity = tempControllerSensitivity;
        // PlayerController.controllerDeadZone = tempControllerDeadZone;
        
        PlayerPrefs.SetInt("resolution", tempResolutionIndex);
        PlayerPrefs.SetInt("fullscreen", tempIsFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat("masterVolume", tempMasterVolume);
        PlayerPrefs.SetFloat("SFXVolume", tempSFXVolume);
        PlayerPrefs.SetFloat("musicVolume", tempMusicVolume);
        PlayerPrefs.SetFloat("mouseSensitivity", tempMouseSensitivity);
        PlayerPrefs.SetFloat("controllerSensitivity", tempControllerSensitivity);
        PlayerPrefs.SetFloat("controllerDeadZone", tempControllerDeadZone);

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
        PlayerPrefs.DeleteKey("controllerSensitivity");
        PlayerPrefs.DeleteKey("controllerDeadZone");
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
        tempControllerSensitivity = PlayerPrefs.GetFloat("controllerSensitivity", 1f);
        tempControllerDeadZone = PlayerPrefs.GetFloat("controllerDeadZone", 0.1f);

        resolutionDropdown.value = tempResolutionIndex;
        fullscreenToggle.isOn = tempIsFullscreen;
        masterVolumeSlider.value = tempMasterVolume;
        SFXVolumeSlider.value = tempSFXVolume;
        musicVolumeSlider.value = tempMusicVolume;
        mouseSensitivitySlider.value = tempMouseSensitivity;
        controllerSensitivitySlider.value = tempControllerSensitivity;
        controllerDeadZoneSlider.value = tempControllerDeadZone;
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
            EventSystem.current.SetSelectedGameObject(controlSchemeUIButton.gameObject);
        }
    }
}