using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractionTutorialUI : MonoBehaviour
{
    public static InteractionTutorialUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public bool IsShowing { get; private set; }

    private System.Action onConfrimCallBack;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        panel.SetActive(false);
        IsShowing = false;
    }

    public void ShowTutorial(string text, System.Action onConfirm)
    {
        if (panel == null || descriptionText == null)
        {
            Debug.LogError("Tutorial UI references missing");
            return;
        }

        descriptionText.text = text;
        panel.SetActive(true);
        descriptionText.gameObject.SetActive(true);

        IsShowing = true;
        onConfrimCallBack = onConfirm;

        Time.timeScale = 0f;
    }

    public void Update()
    {
        if (!IsShowing)
            return;

        // Confirm input (keyboard + gamepad)
        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            HideTutorial();
        }
    }

    public void HideTutorial()
    {
        panel.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        Time.timeScale = 1f;

        IsShowing = false;

        onConfrimCallBack?.Invoke();
        onConfrimCallBack = null;
    }

}
