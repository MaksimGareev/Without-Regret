using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class InteractionTutorialUI : MonoBehaviour
{
    public static InteractionTutorialUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private float fadeDuration = 0.5f;
    private CanvasGroup canvasGroup;

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

        if (panel == null)
        {
            Debug.LogError("Panel reference missing");
            return;
        }

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        panel.SetActive(false);
        IsShowing = false;
    }

    public void ShowTutorial(string text, System.Action onConfirm = null)
    {
        if (panel == null || descriptionText == null)
        {
            Debug.LogError("Tutorial UI references missing");
            return;
        }

        descriptionText.text = text;
        panel.SetActive(true);
        descriptionText.gameObject.SetActive(true);

        StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeDuration));

        IsShowing = true;
        onConfrimCallBack = onConfirm;

        Time.timeScale = 0f;
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.DisableInput();
        }
    }
    /*
    public void ShowBossTutorial(InteractType type, string message)
    {
        if (canvasGroup == null) return;

        descriptionText.text = message;

        gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeDuration));
    }
    */
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
        StartCoroutine(FadeOutAndDeactivate());
    }

    private IEnumerator FadeOutAndDeactivate()
    {
        yield return FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration);

        panel.SetActive(false);
        descriptionText.gameObject.SetActive(false);

        IsShowing = false;

        Time.timeScale = 1f;

        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.EnableInput();
        }

        onConfrimCallBack?.Invoke();
        onConfrimCallBack = null;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        cg.alpha = start;
        cg.interactable = end > 0f;
        cg.blocksRaycasts = end > 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        cg.alpha = end;
        cg.interactable = end > 0f;
        cg.blocksRaycasts = end > 0f;
    }

}
