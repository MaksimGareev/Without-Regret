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
                actionImage1.sprite = controllerXButton;
                actionImage1.rectTransform.sizeDelta = new Vector2(80, 80);

                actionImage2.sprite = controllerAButton;
                actionImage2.rectTransform.sizeDelta = new Vector2(80, 80);
                
                actionImage3.sprite = controllerTrigger;
                actionImage3.rectTransform.sizeDelta = new Vector2(80, 75);
                
                journalImage.sprite = controllerSelect;
                journalImage.rectTransform.sizeDelta = new Vector2(80, 80);
                
                tab.gameObject.SetActive(false);
                e.gameObject.SetActive(false);
                spaceBar.gameObject.SetActive(false);
                break;
            case InputMode.KeyboardMouse:
                actionImage1.sprite = keyboardEKey;
                actionImage1.rectTransform.sizeDelta = new Vector2(80, 80);

                actionImage2.sprite = keyboardSpacebar;
                actionImage2.rectTransform.sizeDelta = new Vector2(140, 85);
                
                actionImage3.sprite = mouse;
                actionImage3.rectTransform.sizeDelta = new Vector2(60, 80);

                journalImage.sprite = keyboardTab;
                journalImage.rectTransform.sizeDelta = new Vector2(140, 85);
                
                tab.gameObject.SetActive(true);
                e.gameObject.SetActive(true);
                spaceBar.gameObject.SetActive(true);
                break;
        }
    }
}
