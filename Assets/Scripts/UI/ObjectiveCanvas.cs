using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup), typeof(AudioSource))]
public class ObjectiveCanvas : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float visibleDuration = 2f;

    [Header("UI References (should already be assigned)")]
    [SerializeField] private GameObject objectiveUI;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private ObjectiveInstance currentObjective;
    private AudioSource audioSource;
    private Coroutine showRoutine;
    private Coroutine hideRoutine;

    private void Awake()
    {
        // If this ObjectiveCanvas is not parented under an ObjectiveManager in the hierarchy,
        // destroy it immediately to avoid multiple instances in one scene
        if (GetComponentInParent<ObjectiveManager>() == null)
        {
            Debug.LogWarning($"ObjectiveCanvas '{name}' destroyed: not a child of an ObjectiveManager.");

            Destroy(gameObject);

            return;
        }

        if (objectiveUI != null)
        {
            objectiveUI.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (titleText == null || descriptionText == null || progressText == null || objectiveUI == null)
        {
            Debug.LogError("ObjectiveCanvas: One or more UI references are not assigned.");
            enabled = false;
            return;
        }
        // Subscribe to manager events when available
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(HandleObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveProgressUpdated.AddListener(HandleObjectiveProgressed);
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
            ObjectiveManager.Instance.OnObjectiveProgressUpdated.RemoveListener(HandleObjectiveProgressed);
            ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(HandleObjectiveCompleted);
        }
    }

    private void HandleObjectiveActivated(ObjectiveInstance newObjective)
    {
        if (newObjective == null)
        {
            Debug.LogError("ObjectiveCanvas: Received null ObjectiveInstance in HandleObjectiveActivated.");
            return;
        }
        currentObjective = newObjective;
        audioSource.Play(); // play scribble sfx
        titleText.text = "New Objective Started!";
        descriptionText.text = newObjective.data.title + ": Check your journal for more information.";
        progressText.text = $"Progress: 0/{newObjective.data.requiredProgress}";

        if (showDebugLogs)
        {
            Debug.Log($"Objective Activated: {newObjective.data.title} - {newObjective.data.description}");
        }

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

        if (showDebugLogs)
        {
            Debug.Log($"Objective Completed: {completedObjective.data.title}");
        }

        showRoutine = StartCoroutine(FadeInUI());
    }

    private void HandleObjectiveProgressed(ObjectiveInstance updatedObjective)
    {
        if (updatedObjective == currentObjective)
        {
            titleText.text = updatedObjective.data.title;
            descriptionText.text = "Objective Progress Updated!";
            progressText.text = $"{updatedObjective.currentProgress} / {updatedObjective.data.requiredProgress}";
        }

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

        if (showDebugLogs)
        {
            Debug.Log($"Objective Progress Updated: {updatedObjective.data.title} - {updatedObjective.currentProgress}/{updatedObjective.data.requiredProgress}");
        }

        showRoutine = StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInUI()
    {
        if (objectiveUI == null)
            yield break;

        if (showDebugLogs)
        {
            Debug.Log("Fading in Objective UI");
        }

        if (!objectiveUI.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            // Ensure visible if there's no canvas group
            objectiveUI.SetActive(true);
            yield break;
        }

        if (IsVisible() && canvasGroup.alpha >= 1f)
        {
            // If already fully visible, go straight to delayedhide
            hideRoutine = StartCoroutine(DelayedHide());
            yield break;
        }

        // If a fade-out is in progress, stop it
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        float duration = Mathf.Max(0f, fadeDuration);
        float startAlpha = (IsVisible() && canvasGroup.alpha > 0f) ? canvasGroup.alpha : 0f;
        canvasGroup.alpha = startAlpha;
        objectiveUI.SetActive(true);

        if (duration <= Mathf.Epsilon)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            // Compute remaining duration proportional to how far we still need to fade
            float remaining = Mathf.Lerp(duration, 0f, startAlpha); // remaining = duration * (1 - startAlpha) approximately
            // Safeguard minimal remaining time
            remaining = Mathf.Max(0.0001f, remaining);

            float elapsed = 0f;
            while (elapsed < remaining)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / remaining);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
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

        if (showDebugLogs)
        {
            Debug.Log("Fading out Objective UI");
        }

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
