using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup), typeof(AudioSource))]
public class ObjectiveCanvas : MonoBehaviour
{
    private enum TransitionPhase
    {
        None,
        FadingIn,
        WaitingToHide,
        FadingOut
    }

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
    
    private TransitionPhase transitionPhase = TransitionPhase.None;
    private float queuedFadeOutAlpha;
    private float remainingHideDelay;
    private float hideDelayStartedAt;
    private bool hasSavedRuntimeState;

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
            objectiveUI.TryGetComponent(out canvasGroup);
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

        if (canvasGroup == null)
        {
            objectiveUI.TryGetComponent(out canvasGroup);
        }

        RestoreRuntimeState();

        // Subscribe to manager events when available
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(HandleObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveProgressUpdated.AddListener(HandleObjectiveProgressed);
            ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(HandleObjectiveCompleted);
            
            var activeObjectives =  ObjectiveManager.Instance.GetActiveObjectives();
            var objectiveInstances = activeObjectives as ObjectiveInstance[] ?? activeObjectives.ToArray();
            if (objectiveInstances.Any())
            {
                currentObjective = objectiveInstances.FirstOrDefault();
            }
        }
        else
        {
            Debug.LogError("ObjectiveUI: ObjectiveManager instance not found.");
        }
    }

    private void OnDisable()
    {
        CacheRuntimeState();

        StopTransitionCoroutines();

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

        StopTransitionCoroutines();

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

        StopTransitionCoroutines();

        if (showDebugLogs)
        {
            Debug.Log($"Objective Completed: {completedObjective.data.title}");
        }

        showRoutine = StartCoroutine(FadeInUI(0f));
    }

    private void HandleObjectiveProgressed(ObjectiveInstance updatedObjective)
    {
        if (updatedObjective.data.objectiveID == currentObjective?.data.objectiveID)
        {
            titleText.text = updatedObjective.data.title;
            descriptionText.text = "Objective Progress Updated!";
            progressText.text = $"{updatedObjective.currentProgress} / {updatedObjective.data.requiredProgress}";
        }

        StopTransitionCoroutines();

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

        transitionPhase = TransitionPhase.FadingIn;
        queuedFadeOutAlpha = Mathf.Clamp01(targetFadeOutAlpha);

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
            showRoutine = null;
            hideRoutine = StartCoroutine(DelayedHide(queuedFadeOutAlpha));
            yield break;
        }

        // If a fade-out is in progress, stop it
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        float duration = Mathf.Max(0f, fadeDuration);
        float startAlpha = (objectiveUI.activeSelf && canvasGroup.alpha > 0f) ? canvasGroup.alpha : 0f;
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
        hideRoutine = StartCoroutine(DelayedHide(queuedFadeOutAlpha));
    }

    private IEnumerator DelayedHide(float targetFadeOutAlpha, float customDelay = -1f)
    {
        transitionPhase = TransitionPhase.WaitingToHide;
        queuedFadeOutAlpha = Mathf.Clamp01(targetFadeOutAlpha);

        float waitDuration = customDelay >= 0f ? customDelay : visibleDuration;
        waitDuration = Mathf.Max(0f, waitDuration);
        remainingHideDelay = waitDuration;
        hideDelayStartedAt = Time.unscaledTime;

        if (waitDuration > Mathf.Epsilon)
        {
            yield return new WaitForSeconds(waitDuration);
        }

        hideRoutine = StartCoroutine(FadeOutUI(queuedFadeOutAlpha));
    }

    private IEnumerator FadeOutUI(float targetAlpha)
    {
        if (objectiveUI == null)
            yield break;

        transitionPhase = TransitionPhase.FadingOut;
        queuedFadeOutAlpha = Mathf.Clamp01(targetAlpha);
        remainingHideDelay = 0f;

        if (showDebugLogs)
        {
            Debug.Log($"Fading out Objective UI to target alpha {queuedFadeOutAlpha}");
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

        targetAlpha = queuedFadeOutAlpha;
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
            transitionPhase = TransitionPhase.None;
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

        transitionPhase = TransitionPhase.None;
        hideRoutine = null;
    }

    private void StopTransitionCoroutines()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
    }

    private void CacheRuntimeState()
    {
        if (objectiveUI == null)
        {
            hasSavedRuntimeState = false;
            return;
        }

        if (canvasGroup == null)
        {
            objectiveUI.TryGetComponent(out canvasGroup);
        }

        if (canvasGroup != null)
        {
            queuedFadeOutAlpha = Mathf.Clamp01(queuedFadeOutAlpha);

            if (transitionPhase == TransitionPhase.WaitingToHide)
            {
                float elapsed = Time.unscaledTime - hideDelayStartedAt;
                remainingHideDelay = Mathf.Max(0f, remainingHideDelay - elapsed);
            }

            hasSavedRuntimeState = true;
        }
    }

    private void RestoreRuntimeState()
    {
        if (!hasSavedRuntimeState || objectiveUI == null || canvasGroup == null)
        {
            return;
        }

        if (!objectiveUI.activeSelf && canvasGroup.alpha > 0f)
        {
            objectiveUI.SetActive(true);
        }

        switch (transitionPhase)
        {
            case TransitionPhase.FadingIn:
                showRoutine = StartCoroutine(FadeInUI(queuedFadeOutAlpha));
                break;
            case TransitionPhase.WaitingToHide:
                hideRoutine = StartCoroutine(DelayedHide(queuedFadeOutAlpha, remainingHideDelay));
                break;
            case TransitionPhase.FadingOut:
                hideRoutine = StartCoroutine(FadeOutUI(queuedFadeOutAlpha));
                break;
        }

        hasSavedRuntimeState = false;
    }

    public bool IsVisible()
    {
        return objectiveUI != null && canvasGroup != null && (canvasGroup.alpha > 0f || objectiveUI.activeSelf);
    }
}
