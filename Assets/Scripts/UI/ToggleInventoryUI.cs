using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject inventoryGameObject;

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction inventoryAction;
    private InputAction cancelAction;

    [Header("Slide Animation Settings")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private Vector2 disabledPosition = new Vector2(-100, -100f);
    [SerializeField] private Vector2 enabledPosition = new Vector2(-100, 120f);
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    public bool isEnabled { get; private set; } = false;
    public bool hasBackpack { get; set; } = false;
    private RectTransform rectTransform;
    private Coroutine slideRoutine;

    void Awake()
    {
        rectTransform = inventoryGameObject.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = disabledPosition;
        inventoryGameObject.SetActive(false);

        // Initialize input actions
        inventoryAction = inputActions.FindAction("Player/Inventory");
        inventoryAction.Enable();

        cancelAction = inputActions.FindAction("UI/Cancel");
        cancelAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (inventoryAction.triggered && hasBackpack && !PauseManager.Instance.isGamePaused && !Journal.Instance.isJournalOpen)
        {
            ToggleInventory();
        }
        else if (cancelAction.triggered && isEnabled)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
        }

        isEnabled = !isEnabled;
        Cursor.visible = !Cursor.visible;

        if (isEnabled)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        slideRoutine = StartCoroutine(SlideInventory(isEnabled));
    }

    private IEnumerator SlideInventory(bool enabled)
    {
        if (enabled)
        {
            inventoryGameObject.SetActive(true);
        }

        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = enabled ? enabledPosition : disabledPosition;

        float timeElapsed = 0f;
        while (timeElapsed < slideDuration)
        {
            timeElapsed += Time.deltaTime;
            float lerpControl = slideCurve.Evaluate(timeElapsed / slideDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, lerpControl);
            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;

        if (!enabled)
        {
            inventoryGameObject.SetActive(false);
        }
    }
}
