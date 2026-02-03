using System;
using UnityEngine;

public static class GameSettings
{
    public static event Action OnSettingsChanged;

    public static float MouseSensitivity { get; private set; } = 1f;
    public static float LeftStickSensitivity { get; private set; } = 1f;
    public static float RightStickSensitivity { get; private set; } = 1f;
    public static float LeftStickDeadZone { get; private set; } = 0.1f;
    public static float RightStickDeadZone { get; private set; } = 0.1f;

    public static void LoadFromPrefs()
    {
        MouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1f);
        LeftStickSensitivity = PlayerPrefs.GetFloat("leftStickSensitivity", 1f);
        RightStickSensitivity = PlayerPrefs.GetFloat("rightStickSensitivity", 1f);
        LeftStickDeadZone = PlayerPrefs.GetFloat("leftStickDeadZone", 0.1f);
        RightStickDeadZone = PlayerPrefs.GetFloat("rightStickDeadZone", 0.1f);
    }

    public static void ApplyControls(float mouseSensitivity, float leftStickSensitivity, float rightStickSensitivity, float leftStickDeadZone, float rightStickDeadZone)
    {
        MouseSensitivity = mouseSensitivity;
        LeftStickSensitivity = leftStickSensitivity;
        RightStickSensitivity = rightStickSensitivity;
        LeftStickDeadZone = leftStickDeadZone;
        RightStickDeadZone = rightStickDeadZone;

        PlayerPrefs.SetFloat("mouseSensitivity", MouseSensitivity);
        PlayerPrefs.SetFloat("leftStickSensitivity", LeftStickSensitivity);
        PlayerPrefs.SetFloat("rightStickSensitivity", RightStickSensitivity);
        PlayerPrefs.SetFloat("leftStickDeadZone", LeftStickDeadZone);
        PlayerPrefs.SetFloat("rightStickDeadZone", RightStickDeadZone);

        OnSettingsChanged?.Invoke();
    }
}
