using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Video;
using System.Collections.Generic;


// Holds all data for a single tutorial
[System.Serializable]
public class TutorialVideoData
{
    public InteractType interactType;   // The type of interaction
    public VideoClip videoClip;         // Video clip corresponding to interaction type

    [TextArea]
    public string tutorialText;         // Text description for tutorial
}

public class InteractionTutorialUI : MonoBehaviour
{
    public static InteractionTutorialUI Instance;

    [Header("Video")]
    [Tooltip("The video player that renders the selected video for the interaction type")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("The list of videos selected to play for tutorial")]
    [SerializeField] private List<TutorialVideoData> tutorialVideos;

    private Dictionary<InteractType, VideoClip> videoLookup;

    [Header("Text")]
    [Tooltip("Root game object containing all UI for tutorial")]
    [SerializeField] private GameObject tutorial;
    [Tooltip("Text element that is changed for specific tutorial descriptions")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Input Dealy")]
    [SerializeField] private float inputDelay = 0.5f;
    private bool canAcceptInput = false;

    private float fadeDuration = 0.5f;      // How long the fade takes
    private CanvasGroup canvasGroup;        // Used to fade UI in and out

    public bool IsShowing { get; private set; } // Tracks if tutorial is currently beeing seen

    private System.Action onConfrimCallBack;    // Optional callback when tutorial is closed

    private void Awake()
    {
        // Buid dictionary for video lookup
        videoLookup = new Dictionary<InteractType, VideoClip>();

        foreach (var entry in tutorialVideos)
        {
            if (!videoLookup.ContainsKey(entry.interactType))
            {
                videoLookup.Add(entry.interactType, entry.videoClip);
            }
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate InteractionTutorialUI destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Safety check to know if UI references exists
        if (tutorial == null)
        {
            Debug.LogError("Panel reference missing");
            return;
        }
        
        // Get or add CanvasGroup
        canvasGroup = tutorial.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tutorial.AddComponent<CanvasGroup>();
        }

        // Start UI fully hidden and non-interactable
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        tutorial.SetActive(false);
        IsShowing = false;
    }

    // Returns the tutorial text for a specific interactType
    // Used by trigger scripts to fetch correct text
    public string GetTutorialText(InteractType type)
    {
        foreach (var entry in tutorialVideos)
        {
            if (entry.interactType == type)
            {
                return entry.tutorialText;
            }
        }

        Debug.LogWarning($"No text found for {type}");
        return "";
    }

    // Shows the Ui with correct text and video
    public void ShowTutorial(InteractType type, string text, System.Action onConfirm = null)
    {
        // diasble other canvases while active
        DisableOtherCanvases();

        if (tutorial == null || descriptionText == null)
        {
            Debug.LogError("Tutorial UI references missing");
            return;
        }

        // Set tutorial text
        descriptionText.text = text;

        if (videoLookup.TryGetValue(type, out VideoClip clip))
        {
            videoPlayer.clip = clip;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning($"No video found for {type}");
        }

        // Enable UI 
        tutorial.SetActive(true);
        descriptionText.gameObject.SetActive(true);

        // Fade UI in
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeDuration));

        IsShowing = true;
        onConfrimCallBack = onConfirm;

        // Pause the game
        Time.timeScale = 0f;

        // Disable player input while tutorial is open
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.DisableInput();
        }

        canAcceptInput = false;
        StartCoroutine(InputDelayRoutine());

    }

    private IEnumerator InputDelayRoutine()
    {
        yield return new WaitForSecondsRealtime(inputDelay);
        canAcceptInput = true;
    }

    public void Update()
    {
        if (!IsShowing || !canAcceptInput)
            return;
        
        if (IsShowing)
        {
            // Confirm input (keyboard + gamepad)
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
            {
                HideTutorial();
            }
        }
    }

    // Hides the tutorial
    public void HideTutorial()
    {
        StartCoroutine(FadeOutAndDeactivate());
        EnableOtherCanvases();
    }

    // Handles fade out and cleanup after tutorial closes
    private IEnumerator FadeOutAndDeactivate()
    {
        // Fade out
        yield return FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration);

        // Stop video
        videoPlayer.Stop();

        // Disable UI
        tutorial.SetActive(false);
        descriptionText.gameObject.SetActive(false);

        IsShowing = false;

        // Resume game
        Time.timeScale = 1f;

        // Re-enable player inputs
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.EnableInput();
        }

        onConfrimCallBack?.Invoke();
        onConfrimCallBack = null;
    }

    // Handles smooth fading of UI using canvas group
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

    private void OnDestroy()
    {
       if (Instance == this)
        {
            Instance = null;
        }
    }

    // Re-enable all other canvases
    private void EnableOtherCanvases()
    {
        Debug.Log("Enabling other canvases from Journal");
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.mainCanvas != null && !GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(true);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && !GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(true);
        }

        if (GameManager.Instance.playerUICanvas != null && !GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(true);
        }

        if (GameManager.Instance.gameOverCanvas != null && !GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(GameOverManager.Instance.IsGameOver);
        }

        if (GameManager.Instance.objectivePanel != null && !GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(GameManager.Instance.objectiveCanvas.IsVisible());
        }
    }

    // Disable all other canvases
    private void DisableOtherCanvases()
    {
        Debug.Log("Disabling other canvases from Journal");
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.mainCanvas != null && GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(false);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(false);
        }

        if (GameManager.Instance.playerUICanvas != null && GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(false);
        }

        if (GameManager.Instance.gameOverCanvas != null && GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(false);
        }

        if (GameManager.Instance.objectivePanel != null && GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(false);
        }
    }

}
