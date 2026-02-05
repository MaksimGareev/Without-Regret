using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ObjectiveCanvas : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject objectiveUI;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float visibleDuration = 2f;

    private ObjectiveInstance currentObjective;
    private CanvasGroup canvasGroup;
    private Coroutine showRoutine;
    private Coroutine hideRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (objectiveUI != null)
        {
            objectiveUI.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (titleText == null || descriptionText == null || progressText == null || objectiveUI == null)
        {
            Debug.LogError("ObjectiveUI: One or more UI references are not assigned.");
            enabled = false;
            return;
        }
        // Subscribe to manager events when available
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(HandleObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(HandleObjectiveCompleted);
        }
        else
        {
            Debug.LogError("ObjectiveUI: ObjectiveManager instance not found.");
        }
    }

    private void OnDisable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.RemoveListener(HandleObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(HandleObjectiveCompleted);
        }
    }

    private void HandleObjectiveActivated(ObjectiveInstance newObjective)
    {
        currentObjective = newObjective;
        RefreshUI(currentObjective);

        // Cancel any pending hide
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }
        showRoutine = StartCoroutine(FadeInUI());
    }

    private void HandleObjectiveCompleted(ObjectiveInstance completedObjective)
    {
        if (completedObjective == currentObjective)
        {
            currentObjective = null;
        }

        // Show a short "completed" notification
        titleText.text = "Objective Complete!";
        descriptionText.text = "Finished: " + completedObjective.data.title;
        progressText.text = "";

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }
        showRoutine = StartCoroutine(FadeInUI());
    }

    private void RefreshUI(ObjectiveInstance objective)
    {
        if (objective != null)
        {
            titleText.text = "New Objective Started!";
            descriptionText.text = objective.data.title + ": Check your journal for more information.";
            progressText.text = $"Progress: 0/{objective.data.requiredProgress}";
        }
        else
        {
            titleText.text = "";
            descriptionText.text = "";
            progressText.text = "";
        }
    }

    void Update()
    {
        if (currentObjective != null)
        {
            progressText.text = $"{currentObjective.currentProgress} / {currentObjective.data.requiredProgress}";
        }
    }

    private IEnumerator FadeInUI()
    {
        if (objectiveUI == null)
            yield break;

        if (!objectiveUI.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            // Ensure visible if there's no canvas group
            objectiveUI.SetActive(true);
            yield break;
        }

        float duration = Mathf.Max(0f, fadeDuration);
        canvasGroup.alpha = 0f;
        objectiveUI.SetActive(true);

        if (duration <= Mathf.Epsilon)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        showRoutine = null;
        hideRoutine = StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(visibleDuration);

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(FadeOutUI());
    }

    private IEnumerator FadeOutUI()
    {
        if (objectiveUI == null)
            yield break;

        if (!objectiveUI.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            objectiveUI.SetActive(false);
            yield break;
        }

        float duration = Mathf.Max(0f, fadeDuration);
        float startAlpha = canvasGroup.alpha;

        if (duration <= Mathf.Epsilon)
        {
            canvasGroup.alpha = 0f;
            objectiveUI.SetActive(false);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        objectiveUI.SetActive(false);
        hideRoutine = null;
    }

    public bool IsVisible()
    {
        return objectiveUI != null && objectiveUI.activeSelf;
    }
}
