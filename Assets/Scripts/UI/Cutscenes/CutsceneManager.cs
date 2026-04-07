using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image dialogueBackgroundImage;
    [SerializeField] private Slider holdToSkipSlider;
    
    [Header("Art Assets")]
    [SerializeField] private Sprite continueButtonSprite;
    [SerializeField] private Sprite dialogueBackgroundSprite;
    
    [Header("Input Action Assets")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction confirmAction;

    [Header("Settings")] 
    [Tooltip("The time in seconds that the player has to hold the confirm input in order to skip the entire cutscene.")]
    [SerializeField, Range(0.5f, 5.0f)] private float holdToSkipDuration = 1.0f;
    private float skipTimer = 0.0f;
    private bool isHoldingSkip = false;

    private bool canSkipClip = false;
    private bool canSkipEntireCutscene = false;
    
    private CutsceneData currentCutscene;
    private int currentClipIndex;
    private Coroutine currentClipCoroutine;
    
    [HideInInspector] public bool isCutscenePlaying = false;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        InitializeInputActions();
    }

    private void Update()
    {
        ProcessHoldToSkip();
    }
    
    private void InitializeInputActions()
    {
        // Initialize input actions
        confirmAction = inputActions.FindActionMap("UI").FindAction("Confirm");
        if (confirmAction== null)
        {
            Debug.LogError("Confirm action not found in InputActionAsset.");
            return;
        }

        confirmAction.started += OnConfirmStarted;
        confirmAction.canceled += OnConfirmCancelled;
        confirmAction.Enable();
    }

    private void ProcessHoldToSkip()
    {
        if (!isHoldingSkip) return;
        
        skipTimer += Time.deltaTime;
        
        if (skipTimer >= holdToSkipDuration && canSkipEntireCutscene)
        {
            EndCutscene();
            ResetHoldTimer();
        }
    }

    private void OnConfirmStarted(InputAction.CallbackContext ctx)
    {
        isHoldingSkip = true;
        skipTimer = 0.0f;
    }

    private void OnConfirmCancelled(InputAction.CallbackContext ctx)
    {
        ResetHoldTimer();
    }

    private void ResetHoldTimer()
    {
        if (skipTimer < holdToSkipDuration && skipTimer > 0.02f && canSkipClip)
        {
            SkipCurrentClip();
        }
        
        isHoldingSkip = false;
        skipTimer = 0.0f;
    }

    public void StartCutscene(CutsceneData cutscene)
    {
        if (!cutscene)
        {
            Debug.LogWarning("No cutscene selected");
            return;
        }

        if (isCutscenePlaying)
        {
            Debug.LogWarning("Cutscene is already playing, ignoring second call to start cutscene");
            return;
        }
        
        currentCutscene = cutscene;

        if (!cutscenePanel.activeSelf)
        {
            cutscenePanel.SetActive(true);
        }
        
        PlayClip(currentCutscene.clips[currentClipIndex]);
        
        isCutscenePlaying = true;

        if (currentCutscene.canSkipEntireCutscene)
        {
            StartCoroutine(WaitToSkipEntireCutscene());
        }
    }

    private IEnumerator WaitToSkipEntireCutscene()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        
        canSkipEntireCutscene = true;
    }

    private void SkipCurrentClip()
    {
        if (currentClipCoroutine != null)
        {
            StopCoroutine(currentClipCoroutine);
        }
        
        HideContinueButton();
        
        currentClipIndex++;
        
        if (currentCutscene.clips.Length <= currentClipIndex)
        {
            EndCutscene();
            return;
        }
        
        PlayClip(currentCutscene.clips[currentClipIndex]);
    }

    private void ShowContinueButton()
    {
        continueButton.gameObject.SetActive(true);
    }

    private void HideContinueButton()
    {
        continueButton.gameObject.SetActive(false);
    }

    private void PlayClip(CutsceneClip clip)
    {
        backgroundImage.sprite = clip.backgroundImage;
        
        // Play appropriate Dialogue
        
        currentClipCoroutine = StartCoroutine(WaitForClipDuration());
    }

    private IEnumerator WaitForClipDuration()
    {
        // Early skip allowing if canSkip overrides duration
        yield return new WaitForSecondsRealtime(0.5f);

        if (currentCutscene.clips[currentClipIndex].canSkipClipEarly)
        {
            canSkipClip = true;
            ShowContinueButton();
        }
        
        yield return new WaitForSecondsRealtime(currentCutscene.clips[currentClipIndex].duration - 0.5f);

        if (currentCutscene.clips[currentClipIndex].autoContinue)
        {
            SkipCurrentClip();
        }
        else
        {
            canSkipClip = true;
            ShowContinueButton();
        }
    }
    
    private void EndCutscene()
    {
        cutscenePanel.SetActive(false);
        
        currentCutscene = null;
        backgroundImage.sprite = null;
        
        isCutscenePlaying = false;
    }
}
