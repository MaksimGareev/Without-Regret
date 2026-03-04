using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using TMPro;
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

    private IDisposable inputSubscription;

    [Header("UI References")]
    [SerializeField] private RawImage actionImage1;
    [SerializeField] private RawImage actionImage2;
    [SerializeField] private RawImage actionImage3;
    [SerializeField] private Image journalImage;

    [Header("Controller Sprites")]
    [SerializeField] private Texture controllerXButton;
    [SerializeField] private Texture controllerAButton;
    [SerializeField] private Texture controllerTrigger;
    [SerializeField] private Sprite controllerSelect;

    [Header("Keyboard Sprites")]
    [SerializeField] private Texture keyboardEKey;
    [SerializeField] private Texture keyboardSpacebar;
    [SerializeField] private Texture mouse;
    [SerializeField] private Sprite keyboardTab;
    
    [Header("Keyboard text")]
    [SerializeField] private TextMeshProUGUI tab;
    [SerializeField] private TextMeshProUGUI e;
    [SerializeField] private TextMeshProUGUI spaceBar;

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

    private void OnEnable()
    {
        //inputSubscription = InputSystem.onAnyButtonPress.Subscribe(control => DetectInputDevice(control);
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDisable()
    {
        //inputSubscription?.Dispose();
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
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
                actionImage1.texture = controllerXButton;
                actionImage2.texture = controllerAButton;
                actionImage3.texture = controllerTrigger;
                
                journalImage.sprite = controllerSelect;
                journalImage.rectTransform.sizeDelta = new Vector2(80, 80);
                
                tab.gameObject.SetActive(false);
                e.gameObject.SetActive(false);
                spaceBar.gameObject.SetActive(false);
                break;
            case InputMode.KeyboardMouse:
                actionImage1.texture = keyboardEKey;
                actionImage2.texture = keyboardSpacebar;
                actionImage3.texture = mouse;

                journalImage.sprite = keyboardTab;
                journalImage.rectTransform.sizeDelta = new Vector2(140, 85);
                
                tab.gameObject.SetActive(true);
                e.gameObject.SetActive(true);
                spaceBar.gameObject.SetActive(true);
                break;
        }
    }
}
