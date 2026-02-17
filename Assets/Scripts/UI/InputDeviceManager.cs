using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using System;

public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }

    public enum InputMode
    {
        KeyboardMouse,
        Controller
    }

    public InputMode CurrentMode { get; private set; } = InputMode.Controller;

    public event Action<InputMode> OnInputModeChanged;

    [Header("UI References")]
    [SerializeField] private RawImage actionImage1;
    [SerializeField] private RawImage actionImage2;

    [Header("Controller Sprites")]
    [SerializeField] private Texture controllerXButton;
    [SerializeField] private Texture controllerAButton;

    [Header("Keyboard Sprites")]
    [SerializeField] private Texture keyboardEKey;
    [SerializeField] private Texture keyboardSpacebar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        DetectInputDevice();
    }

    private void DetectInputDevice()
    {
        InputMode previousMode = CurrentMode;

        bool keyboardUsed = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) || (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero);
        bool controllerUsed = false;

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    controllerUsed = true;
                    break;
                }
            }

            if (!controllerUsed)
            {
                var leftStick = Gamepad.current.leftStick.ReadValue();
                var rightStick = Gamepad.current.rightStick.ReadValue();
                if (leftStick.magnitude > 0.1f || rightStick.magnitude > 0.1f || Gamepad.current.leftTrigger.ReadValue() > 0.1f || Gamepad.current.rightTrigger.ReadValue() > 0.1f)
                {
                    controllerUsed = true;
                }
            }
        }

        if (keyboardUsed)
        {
            CurrentMode = InputMode.KeyboardMouse;
        }
        else if (controllerUsed)
        {
            CurrentMode = InputMode.Controller;
        }

        // only invoke event if device changed
        if (previousMode != CurrentMode)
        {
            OnInputModeChanged?.Invoke(CurrentMode);
            UpdateUIForInputMode(CurrentMode);
        }
    }

    private void UpdateUIForInputMode(InputMode mode)
    {
        switch (mode)
        {
            case InputMode.Controller:
                if (actionImage1 != null) actionImage1.texture = controllerXButton;
                if (actionImage2 != null) actionImage2.texture = controllerAButton;
                break;
            case InputMode.KeyboardMouse:
                if (actionImage1 != null) actionImage1.texture = keyboardEKey;
                if (actionImage2 != null) actionImage2.texture = keyboardSpacebar;
                break;
        }
    }
}
