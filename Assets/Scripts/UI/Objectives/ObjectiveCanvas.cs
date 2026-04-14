using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup), typeof(AudioSource))]
public class ObjectiveCanvas : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("How long fading the UI in/out takes until it's at 100 or 0 opacity")] private float fadeDuration = 0.5f;
    [SerializeField, Tooltip("How long the UI is visible for (time starts as soon as it's fully visible)")] private float visibleDuration = 2f;
    [SerializeField, Tooltip("The alpha the objective popup fades to after objective activation/progression")] private float targetAlpha = 0.5f;

    [Header("UI References (should already be assigned)")]
    [SerializeField] private GameObject objectiveUI;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private CanvasGroup canvasGroup;
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

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            audioSource.Play(); // play scribble sfx
        }

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
        showRoutine = StartCoroutine(FadeInUI(targetAlpha));
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

        showRoutine = StartCoroutine(FadeInUI(0f));
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

        showRoutine = StartCoroutine(FadeInUI(targetAlpha));
    }

    private IEnumerator FadeInUI(float targetFadeOutAlpha)
    {
        if (objectiveUI == null)
            yield break;

        if (showDebugLogs)
        {
            Debug.Log("Fading in Objective UI");
        }

        if (!objectiveUI.TryGetComponent<CanvasGroup>(out canvasGroup))
        {
            // Ensure visible if there's no canvas group
            objectiveUI.SetActive(true);
            yield break;
        }

        if (IsVisible() && canvasGroup.alpha >= 1f)
        {
            // If already fully visible, go straight to delayedhide
            hideRoutine = StartCoroutine(DelayedHide(targetFadeOutAlpha));
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
        hideRoutine = StartCoroutine(DelayedHide(targetFadeOutAlpha));
    }

    private IEnumerator DelayedHide(float targetFadeOutAlpha)
    {
        yield return new WaitForSeconds(visibleDuration);

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(FadeOutUI(targetFadeOutAlpha));
    }

    private IEnumerator FadeOutUI(float targetAlpha)
    {
        if (objectiveUI == null)
            yield break;

        if (showDebugLogs)
        {
            Debug.Log($"Fading out Objective UI to target alpha {targetAlpha}");
        }

        if (!objectiveUI.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            if (targetAlpha <= 0f)
            {
                objectiveUI.SetActive(false);
            }
            else
            {
                objectiveUI.SetActive(true);
            }
            hideRoutine = null;
            yield break;
        }

        targetAlpha = Mathf.Clamp01(targetAlpha);
        float duration = Mathf.Max(0f, fadeDuration);
        float startAlpha = canvasGroup.alpha;

        // If no time to fade or already at target, set final and handle active state
        if (duration <= Mathf.Epsilon || Mathf.Approximately(startAlpha, targetAlpha))
        {
            canvasGroup.alpha = targetAlpha;
            if (targetAlpha <= 0f)
            {
                objectiveUI.SetActive(false);
            }
            hideRoutine = null;
            yield break;
        }

        // Scale duration by how much alpha actually needs to change so transitions are proportional
        float remaining = duration * Mathf.Abs(startAlpha - targetAlpha);
        remaining = Mathf.Max(0.0001f, remaining);

        float elapsed = 0f;
        while (elapsed < remaining)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / remaining);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // Only deactivate when fully transparent
        if (targetAlpha <= 0f)
        {
            objectiveUI.SetActive(false);
        }

        hideRoutine = null;
    }

    public bool IsVisible()
    {
        return objectiveUI != null && canvasGroup != null && (canvasGroup.alpha > targetAlpha || objectiveUI.activeSelf);
    }
}
