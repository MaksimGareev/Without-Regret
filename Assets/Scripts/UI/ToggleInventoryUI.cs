using System.Collections;
using UnityEngine;

public class ToggleInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject inventoryGameObject;
    [SerializeField] private KeyCode inventoryKey;
    [SerializeField] private string inventoryButton;

    [Header("Slide Animation Settings")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private Vector2 disabledPosition = new Vector2(-100, -100f);
    [SerializeField] private Vector2 enabledPosition = new Vector2(-100, 120f);
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    public bool isEnabled { get; private set; } = false;
    private RectTransform rectTransform;
    private Coroutine slideRoutine;

    void Awake()
    {
        rectTransform = inventoryGameObject.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = disabledPosition;
        inventoryGameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(inventoryKey) || Input.GetButtonDown(inventoryButton))
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
