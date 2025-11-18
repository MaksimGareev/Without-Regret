using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MMSettings : MonoBehaviour
{
    [Header("Settings References")]
    [SerializeField] private TMP_Dropdown graphicsQualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider controllerSensitivitySlider;
    [SerializeField] private Button resetButton;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI masterVolumeValueText;
    [SerializeField] private TextMeshProUGUI mouseSensitivityValueText;
    [SerializeField] private TextMeshProUGUI controllerSensitivityValueText;

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
        graphicsQualityDropdown.onValueChanged.AddListener(SetGraphicsQuality);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        controllerSensitivitySlider.onValueChanged.AddListener(SetControllerSensitivity);
        resetButton.onClick.AddListener(ResetSettings);
    }

    public void SetGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("quality", index);
    }

    public void SetResolution(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resolution", index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetMasterVolume(float volume)
    {
        // Assuming an AudioManager exists to handle volume
        AudioListener.volume = volume;
        masterVolumeValueText.text = Mathf.RoundToInt(volume * 100).ToString("F0") + "%";
        PlayerPrefs.SetFloat("masterVolume", volume);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        // Assuming a PlayerController exists to handle mouse sensitivity
        // PlayerController.mouseSensitivity = sensitivity;
        mouseSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        PlayerPrefs.SetFloat("mouseSensitivity", sensitivity);
    }

    public void SetControllerSensitivity(float sensitivity)
    {
        // Assuming a PlayerController exists to handle controller sensitivity
        // PlayerController.controllerSensitivity = sensitivity;
        controllerSensitivityValueText.text = Mathf.RoundToInt(sensitivity * 100).ToString("F0") + "%";
        PlayerPrefs.SetFloat("controllerSensitivity", sensitivity);
    }

    public void ResetSettings()
    {
        PlayerPrefs.DeleteKey("quality");
        PlayerPrefs.DeleteKey("resolution");
        PlayerPrefs.DeleteKey("fullscreen");
        PlayerPrefs.DeleteKey("masterVolume");
        PlayerPrefs.DeleteKey("mouseSensitivity");
        PlayerPrefs.DeleteKey("controllerSensitivity");
        LoadSettings();
    }

    private void LoadSettings()
    {
        int quality = PlayerPrefs.GetInt("quality", 0);
        graphicsQualityDropdown.value = quality;
        QualitySettings.SetQualityLevel(quality);

        int resolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt("resolution", resolutions.Length - 1), 0, resolutions.Length - 1);
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();
        SetResolution(resolutionIndex);

        bool isFullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;

        float masterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
        masterVolumeSlider.value = masterVolume;
        AudioListener.volume = masterVolume;

        float mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1f);
        mouseSensitivitySlider.value = mouseSensitivity;

        float controllerSensitivity = PlayerPrefs.GetFloat("controllerSensitivity", 1f);
        controllerSensitivitySlider.value = controllerSensitivity;
    }
}
