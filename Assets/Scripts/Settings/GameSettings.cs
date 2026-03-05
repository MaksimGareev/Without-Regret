using System;
using UnityEngine;

public static class GameSettings
{
    public static event Action OnSettingsChanged;

    public static int MouseSensitivity { get; private set; } = 100;
    public static int LeftStickSensitivity { get; private set; } = 100;
    public static int RightStickSensitivity { get; private set; } = 100;
    public static int LeftStickDeadZone { get; private set; } = 10;
    public static int RightStickDeadZone { get; private set; } = 10;

    public static void LoadFromPrefs()
    {
        MouseSensitivity = PlayerPrefs.GetInt("mouseSensitivity", 100);
        LeftStickSensitivity = PlayerPrefs.GetInt("leftStickSensitivity", 100);
        RightStickSensitivity = PlayerPrefs.GetInt("rightStickSensitivity", 100);
        LeftStickDeadZone = PlayerPrefs.GetInt("leftStickDeadZone", 10);
        RightStickDeadZone = PlayerPrefs.GetInt("rightStickDeadZone", 10);
    }

    public static void ApplyControls(int mouseSensitivity, int leftStickSensitivity, int rightStickSensitivity, int leftStickDeadZone, int rightStickDeadZone)
    {
        MouseSensitivity = mouseSensitivity;
        LeftStickSensitivity = leftStickSensitivity;
        RightStickSensitivity = rightStickSensitivity;
        LeftStickDeadZone = leftStickDeadZone;
        RightStickDeadZone = rightStickDeadZone;

        PlayerPrefs.SetInt("mouseSensitivity", MouseSensitivity);
        PlayerPrefs.SetInt("leftStickSensitivity", LeftStickSensitivity);
        PlayerPrefs.SetInt("rightStickSensitivity", RightStickSensitivity);
        PlayerPrefs.SetInt("leftStickDeadZone", LeftStickDeadZone);
        PlayerPrefs.SetInt("rightStickDeadZone", RightStickDeadZone);

        OnSettingsChanged?.Invoke();
    }
}
