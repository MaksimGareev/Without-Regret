using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

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

    private GameObject activeUIObject = null;
    private bool UIActive = false;

    [Header("Input Mode Switching")]
    [SerializeField, Min(0f)] private float controllerMovementThreshold = 0.2f;
    [Min(0f)] private float mouseMovementThreshold = 0.01f;
    [SerializeField, Min(0f)] private float inputModeSwitchCooldown = 0.1f;
    private float lastInputModeSwitchTime = float.NegativeInfinity;
    
    [Header("InputActions")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("UI References")]
    [SerializeField] private Image actionImage1;
    [SerializeField] private Image actionImage2;
    [SerializeField] private Image actionImage3;
    [SerializeField] private Image journalImage;

    [Header("Controller Sprites")]
    [SerializeField] private Sprite controllerXButton;
    [SerializeField] private Sprite controllerAButton;
    [SerializeField] private Sprite controllerTrigger;
    [SerializeField] private Sprite controllerSelect;

    [Header("Keyboard Sprites")]
    [SerializeField] private Sprite keyboardEKey;
    [SerializeField] private Sprite keyboardSpacebar;
    [SerializeField] private Sprite mouse;
    [SerializeField] private Sprite keyboardTab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Stay subscribed even if this GameObject gets temporarily disabled by UI toggles.
        InputSystem.onEvent -= OnInputEvent;
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDestroy()
    {
        InputSystem.onEvent -= OnInputEvent;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        EnsurePlayerInputActive();
        
        if (!UIActive || CurrentMode != InputMode.Controller)
            return;

        EnsureControllerSelection();
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device == null) return;

        if (!TryGetInputMode(eventPtr, device, out InputMode targetMode))
        {
            return;
        }

        if (CurrentMode != targetMode && Time.unscaledTime - lastInputModeSwitchTime < inputModeSwitchCooldown)
        {
            return;
        }

        InputMode previous = CurrentMode;
        CurrentMode = targetMode;

        if (previous != CurrentMode)
        {
            lastInputModeSwitchTime = Time.unscaledTime;
            OnInputModeChanged?.Invoke(CurrentMode);
            UpdateUIForInputMode(CurrentMode);
        }
    }

    private void EnsurePlayerInputActive()
    {
        if (UIActive || !inputActions) return;
        
        var playerMap =  inputActions.FindActionMap("Player");

        if (playerMap == null) return;
        
        if(playerMap.enabled) return;
        
        playerMap.Enable();
        Debug.LogWarning("Player Input was not active outside of UI, re-enabling it now.");
        
        var pauseAction = playerMap.FindAction("Pause");
        var journalAction = playerMap.FindAction("Journal");
        
        if (pauseAction != null && !pauseAction.enabled) pauseAction.Enable();
        if (journalAction != null && !journalAction.enabled) journalAction.Enable();
    }

    private bool TryGetInputMode(InputEventPtr eventPtr, InputDevice device, out InputMode mode)
    {
        mode = CurrentMode;

        if (device is Keyboard)
        {
            mode = InputMode.KeyboardMouse;
            return true;
        }

        if (device is Mouse mouseDevice)
        {
            if (HasPressedAnyButtonInEvent(mouseDevice, eventPtr) || IsMeaningfulMouseMovement(mouseDevice, eventPtr))
            {
                mode = InputMode.KeyboardMouse;
                return true;
            }
        }

        else if (device is Gamepad gamepad)
        {
            if (HasPressedAnyButtonInEvent(gamepad, eventPtr) || IsMeaningfulGamepadMovement(gamepad, eventPtr))
            {
                mode = InputMode.Controller;
                return true;
            }
        }

        return false;
    }

    private bool IsMeaningfulGamepadMovement(Gamepad gamepad, InputEventPtr eventPtr)
    {
        float thresholdSqr = controllerMovementThreshold * controllerMovementThreshold;
        bool hasLeftStick = gamepad.leftStick.ReadValueFromEvent(eventPtr, out Vector2 leftStick);
        bool hasRightStick = gamepad.rightStick.ReadValueFromEvent(eventPtr, out Vector2 rightStick);
        bool hasDPad = gamepad.dpad.ReadValueFromEvent(eventPtr, out Vector2 dpad);
        bool hasLeftTrigger = gamepad.leftTrigger.ReadValueFromEvent(eventPtr, out float leftTrigger);
        bool hasRightTrigger = gamepad.rightTrigger.ReadValueFromEvent(eventPtr, out float rightTrigger);

        return (hasLeftStick && leftStick.sqrMagnitude >= thresholdSqr)
            || (hasRightStick && rightStick.sqrMagnitude >= thresholdSqr)
            || (hasDPad && dpad.sqrMagnitude >= thresholdSqr)
            || (hasLeftTrigger && leftTrigger >= controllerMovementThreshold)
            || (hasRightTrigger && rightTrigger >= controllerMovementThreshold);
    }

    private bool IsMeaningfulMouseMovement(Mouse mouseDevice, InputEventPtr eventPtr)
    {
        float thresholdSqr = mouseMovementThreshold * mouseMovementThreshold;
        bool hasDelta = mouseDevice.delta.ReadValueFromEvent(eventPtr, out Vector2 delta);
        bool hasScroll = mouseDevice.scroll.ReadValueFromEvent(eventPtr, out Vector2 scroll);

        return (hasDelta && delta.sqrMagnitude >= thresholdSqr)
            || (hasScroll && scroll.sqrMagnitude > 0f);
    }

    private static bool HasPressedAnyButtonInEvent(InputDevice device, InputEventPtr eventPtr)
    {
        foreach (InputControl control in device.allControls)
        {
            if (control is ButtonControl button
                && button.ReadValueFromEvent(eventPtr, out float value)
                && value > 0f)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateUIForInputMode(InputMode mode)
    {
        switch (mode)
        {
            case InputMode.Controller:
                
                // Player UI Sprite Updates
                actionImage1.sprite = controllerXButton;
                actionImage1.rectTransform.sizeDelta = new Vector2(80, 80);

                actionImage2.sprite = controllerAButton;
                actionImage2.rectTransform.sizeDelta = new Vector2(80, 80);
                
                actionImage3.sprite = controllerTrigger;
                actionImage3.rectTransform.sizeDelta = new Vector2(80, 75);
                
                journalImage.sprite = controllerSelect;
                journalImage.rectTransform.sizeDelta = new Vector2(80, 80);

                
                // Updates for controller input
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }

                if (Cursor.visible)
                {
                    Cursor.visible = false;
                }

                if (UIActive)
                {
                    EnsureControllerSelection();
                }
                
                break;
            
            case InputMode.KeyboardMouse:
                
                // Player UI Sprite Updates
                actionImage1.sprite = keyboardEKey;
                actionImage1.rectTransform.sizeDelta = new Vector2(80, 80);

                actionImage2.sprite = keyboardSpacebar;
                actionImage2.rectTransform.sizeDelta = new Vector2(140, 85);
                
                actionImage3.sprite = mouse;
                actionImage3.rectTransform.sizeDelta = new Vector2(60, 80);

                journalImage.sprite = keyboardTab;
                journalImage.rectTransform.sizeDelta = new Vector2(140, 85);
                
                // Updates for mouse and keyboard input
                if (UIActive)
                {
                    if (Cursor.lockState != CursorLockMode.None)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }

                    if (!Cursor.visible)
                    {
                        Cursor.visible = true;
                    }

                    ClearSelection();
                }
                else
                {
                    if (Cursor.lockState != CursorLockMode.Locked)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    if (Cursor.visible)
                    {
                        Cursor.visible = false;
                    }
                }
                
                break;
        }
    }
    
    private void EnsureControllerSelection()
    {
        EventSystem eventSystem = EventSystem.current;
        if (!eventSystem || eventSystem.currentSelectedGameObject)
        {
            return;
        }

        GameObject target = eventSystem.firstSelectedGameObject;

        if ((!target || !target.activeInHierarchy) && activeUIObject)
        {
            Selectable selectable = activeUIObject.GetComponentInChildren<Selectable>(true);
            if (selectable)
            {
                target = selectable.gameObject;
            }
        }

        if (target && target.activeInHierarchy)
        {
            eventSystem.SetSelectedGameObject(target);
        }
    }

    private void ClearSelection()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem && eventSystem.currentSelectedGameObject)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    public void SetUIActive(bool active, GameObject uiRoot)
    {
        bool hasContextChanged = UIActive != active || activeUIObject != uiRoot;

        UIActive = active;
        if (active)
        {
            this.activeUIObject = uiRoot;
        }
        else
        {
            this.activeUIObject = null;

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Cursor.visible)
            {
                Cursor.visible = false;
            }
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem)
            {
                eventSystem.SetSelectedGameObject(null);
                eventSystem.firstSelectedGameObject = null;
            }
        }

        if (!hasContextChanged)
        {
            return;
        }

        // Re-apply the current mode when UI context changes (eg. pause opens while already on controller).
        OnInputModeChanged?.Invoke(CurrentMode);
        UpdateUIForInputMode(CurrentMode);
    }
}
