using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
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
        if (!UIActive || CurrentMode != InputMode.Controller)
            return;

        EnsureControllerSelection();
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device == null) return;

        InputMode previous = CurrentMode;

        if (device is Gamepad)
        {
            CurrentMode = InputMode.Controller;
        }
        else if (device is Keyboard || device is Mouse)
        {
            CurrentMode = InputMode.KeyboardMouse;
        }

        if (previous != CurrentMode)
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
        UIActive = active;
        if (active)
        {
            this.activeUIObject = uiRoot;
            inputActions.FindActionMap("UI")?.Enable();
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
            
            inputActions.FindActionMap("UI")?.Disable();
        }

        // Re-apply the current mode when UI context changes (eg. pause opens while already on controller).
        OnInputModeChanged?.Invoke(CurrentMode);
        UpdateUIForInputMode(CurrentMode);
    }
}
