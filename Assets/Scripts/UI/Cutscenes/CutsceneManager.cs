using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }
    
    [Header("General Settings")]
    [Tooltip("The time in seconds that it will take for the cutscene to fade in and out when starting and ending the cutscene")]
    [SerializeField, Range(0.0f, 5.0f)] private float fadeDuration = 0.5f;
    
    [Tooltip("The time in seconds that it will take to transition between two clips")]
    [SerializeField, Range(0.0f, 5.0f)] private float transitionDuration = 0.5f;
    
    [Tooltip("The time in seconds that the player has to hold the confirm input in order to skip the entire cutscene.")]
    [SerializeField, Range(0.5f, 5.0f)] private float holdToSkipDuration = 1.0f;
    private float skipTimer = 0.0f;
    private bool isHoldingSkip = false;
    
    [Header("UI References")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private RawImage primaryBackgroundImage;
    [SerializeField] private RawImage secondaryBackgroundImage; 
    [SerializeField] private Image dialogueBackgroundImage;
    [SerializeField] private Image speakerNameBackgroundImage;
    [SerializeField] private GameObject holdToSkipPanel;
    [SerializeField] private Slider holdToSkipSlider;
    [SerializeField] private TextMeshProUGUI holdText;
    [SerializeField] private Image  holdKeyImage;
    [SerializeField] private TextMeshProUGUI toSkipText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    
    [Header("Art Assets")]
    [SerializeField] private Sprite holdButtonSprite;
    [SerializeField] private Vector3 holdButtonScale =  Vector3.one;
    [SerializeField] private Sprite holdKeySprite;
    [SerializeField] private Vector3 holdKeyScale = Vector3.one;
    [SerializeField]private Vector3 holdTextOffset = Vector3.zero ;
    [SerializeField]private Vector3 toSkipTextOffset = Vector3.zero;
    [SerializeField] private Sprite continueButtonSprite;
    [SerializeField] private Sprite dialogueBackgroundSprite;
    [SerializeField] private Sprite speakerBackgroundSprite;
    private Vector3 originalHoldTextOffset;
    private Vector3 originalToSkipOffset;
    
    [Header("Audio")]
    [SerializeField] private AudioMixer mainAudioMixer;
    
    [Tooltip("Audio clips of each letter A-Z")]
    [SerializeField] private List<AudioClip> letterClips;
    
    private readonly Dictionary<char, AudioClip> letterSounds = new();
    private AudioSource backgroundAudioSource;
    private AudioSource dialogueAudioSource;
    private AudioSource soundEffectAudioSource;
    
    [Header("Input Action Assets")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction confirmAction;
    //private bool usingController = true;
    private bool usingControllerLegend = true;

    private bool canSkipClip = false;
    private bool canSkipEntireCutscene = false;
    
    private CutsceneData currentCutscene;
    private int currentClipIndex;
    private Coroutine currentClipCoroutine;

    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Coroutine fadeInCoroutine;
    private Coroutine fadeOutCoroutine;
    private Coroutine transitionBackgroundCoroutine;
    private Coroutine fadeOtherAudioCoroutine;
    
    [HideInInspector] public bool isCutscenePlaying = false;
    
    private CanvasGroup cutsceneCanvasGroup;
    
    private Dictionary<AudioSource, float> otherAudioSources =  new Dictionary<AudioSource, float>();

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        BuildLetterSounds();
    }
    
    void BuildLetterSounds()
    {
        for (int i = 0; i < letterClips.Count; i++)
        {
            letterSounds[(char)('A' + i)] = letterClips[i];
        }
    }

    private void OnEnable()
    {
        InitializeInputActions();
        InitializeAudioSources();
        InitializeUIArt();

        if (holdToSkipSlider)
        {
            holdToSkipSlider.maxValue = holdToSkipDuration;
            holdToSkipSlider.value = 0.0f;
        }

        if (!mainAudioMixer)
        {
            Debug.LogError("AudioMixer not found in CutsceneManager. Please assign the \"MainMixer\" audio mixer to the CutsceneManager.");
        }
        
        if (!letterSounds.Count.Equals(26))
        {
            Debug.LogWarning("Letter sounds not found in CutsceneManager. Please assign letter sounds to the letterSounds dictionary in the CutsceneManager.");
        }
        
        if (cutscenePanel)
        {
            cutsceneCanvasGroup = cutscenePanel.GetComponent<CanvasGroup>();
            
            if (!cutsceneCanvasGroup)
            {
                Debug.LogWarning("Cutscene Panel is missing a CanvasGroup component. Adding one now.");
                cutsceneCanvasGroup = cutscenePanel.AddComponent<CanvasGroup>();
            }
            
            cutscenePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Cutscene Manager requires a Cutscene Panel.");
        }
        
        if (holdToSkipPanel)
        {
            holdToSkipPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Cutscene Manager requires a Hold To Skip Panel.");
        }
        
        originalHoldTextOffset = holdText.rectTransform.anchoredPosition;
        originalToSkipOffset = toSkipText.rectTransform.anchoredPosition;

        if (ObjectiveManager.Instance)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(OnObjectiveCompleted);
        }
        
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.OnInputModeChanged += OnInputModeChanged;
            Debug.Log("Cutscene Manager subscribed to InputDeviceManager.OnInputModeChanged");
            OnInputModeChanged(InputDeviceManager.Instance.CurrentMode);
        }
        else
        {
            StartCoroutine(WaitUntilIDMReady());
        }
    }

    private IEnumerator WaitUntilIDMReady()
    {
        while (!InputDeviceManager.Instance)
        {
            yield return null;
        }
        
        InputDeviceManager.Instance.OnInputModeChanged += OnInputModeChanged;
        Debug.Log("Cutscene Manager subscribed to InputDeviceManager.OnInputModeChanged");
        OnInputModeChanged(InputDeviceManager.Instance.CurrentMode);
    }

    private void InitializeUIArt()
    {
        if (dialogueBackgroundImage && dialogueBackgroundSprite)
        {
            dialogueBackgroundImage.sprite = dialogueBackgroundSprite;
        }
        
        if (speakerNameBackgroundImage && speakerBackgroundSprite)
        {
            speakerNameBackgroundImage.sprite = speakerBackgroundSprite;
        }
    }
    
    private void OnObjectiveCompleted(ObjectiveInstance objectiveInstance)
    {
        if (objectiveInstance.data.cutsceneOnCompletion)
        {
            StartCutscene(objectiveInstance.data.cutsceneOnCompletion);
        }
    }

    private void InitializeAudioSources()
    {
        var audioSources = GetComponents<AudioSource>();
        
        if (audioSources.Length < 3)
        {
            Debug.LogError("CutsceneManager requires at least 3 AudioSources. Please add more AudioSources to the CutsceneManager GameObject.");
            return;
        }
        
        backgroundAudioSource = audioSources[0];
        dialogueAudioSource = audioSources[1];
        soundEffectAudioSource = audioSources[2];

        if (!mainAudioMixer) return;
        
        // Auto assign mixer groups for volume control
        backgroundAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("Music")[0];
        dialogueAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("Dialogue")[0];
        soundEffectAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("SFX")[0];
    }
    
    private void InitializeInputActions()
    {
        // Initialize input actions
        confirmAction = inputActions.FindActionMap("UI").FindAction("CutsceneConfirm");
        if (confirmAction != null)
        {
            confirmAction.started += OnConfirmStarted;
            confirmAction.canceled += OnConfirmCancelled;
            confirmAction.Enable();
        }
        else
        {
            Debug.LogError("Confirm action not found in InputActionAsset.");
        }

        if (continueButton)
        {
            continueButton.onClick.AddListener(SkipCurrentClip);
        }
    }
    
    private void Update()
    {
        ProcessHoldToSkip();
    }

    private void ProcessHoldToSkip()
    {
        if (!isHoldingSkip)
        {
            if (holdToSkipSlider.gameObject.activeSelf)
            {
                holdToSkipSlider.gameObject.SetActive(false);
            }
            return;
        }
        
        skipTimer += Time.unscaledDeltaTime;
        
        if (holdToSkipSlider)
        {
            if (skipTimer > 0.2f)
            {
                holdToSkipSlider.gameObject.SetActive(true);
            }
            
            holdToSkipSlider.value = skipTimer;
        }
        
        if (skipTimer >= holdToSkipDuration && canSkipEntireCutscene)
        {
            holdToSkipPanel.SetActive(false);
            if (isCutscenePlaying)
            {
                EndCutscene();
            }
            ResetHoldTimer();
        }
    }

    private void OnInputModeChanged(InputDeviceManager.InputMode inputMode)
    {
        switch (inputMode)
        {
            case InputDeviceManager.InputMode.KeyboardMouse:
                if (usingControllerLegend)
                {
                    SwitchToMouseKeyLegend();
                }
                break;
            
            case InputDeviceManager.InputMode.Controller:
                if (!usingControllerLegend)
                {
                    SwitchToControllerLegend();
                }
                break;
        }
    }

    private void SwitchToControllerLegend()
    {
        holdKeyImage.sprite = holdButtonSprite;
        holdKeyImage.SetNativeSize();
        holdKeyImage.rectTransform.localScale = holdButtonScale;
        
        holdText.rectTransform.anchoredPosition = originalHoldTextOffset;
        toSkipText.rectTransform.anchoredPosition = originalToSkipOffset;
        
        usingControllerLegend = true;
    }

    private void SwitchToMouseKeyLegend()
    {
        holdKeyImage.sprite = holdKeySprite;
        holdKeyImage.SetNativeSize();
        holdKeyImage.rectTransform.localScale = holdKeyScale;
        
        holdText.rectTransform.anchoredPosition = holdTextOffset;
        toSkipText.rectTransform.anchoredPosition = toSkipTextOffset;
        
        usingControllerLegend = false;
    }

    private void OnConfirmStarted(InputAction.CallbackContext ctx)
    {
        isHoldingSkip = true;
        skipTimer = 0.0f;

        if (holdToSkipSlider && canSkipEntireCutscene)
        {
            holdToSkipSlider.value = 0.0f;
        }
    }

    private void OnConfirmCancelled(InputAction.CallbackContext ctx)
    {
        ResetHoldTimer();
    }

    private void ResetHoldTimer()
    {
        if (skipTimer < holdToSkipDuration && skipTimer > 0.02f && canSkipClip && isCutscenePlaying)
        {
            SkipCurrentClip();
        }
        
        if (holdToSkipSlider)
        {
            holdToSkipSlider.gameObject.SetActive(false);
            holdToSkipSlider.value = 0.0f;
        }
        
        isHoldingSkip = false;
        skipTimer = 0.0f;
    }

    public bool StartCutscene(CutsceneData cutscene)
    {
        // Do nothing if cutscene is null or if a cutscene is already playing to prevent overlapping cutscenes and null reference errors
        if (!cutscene)
        {
            Debug.LogWarning("No cutscene selected");
            return false;
        }

        if (isCutscenePlaying)
        {
            Debug.LogWarning("Cutscene is already playing, ignoring second call to start cutscene");
            return false;
        }
        
        EnableInput(); 
        
        Time.timeScale = 0f; // Pause the game while the cutscene is playing
        
        currentCutscene = cutscene;

        if (!cutscenePanel.activeSelf)
        {
            cutscenePanel.SetActive(true);
        }

        if (cutsceneCanvasGroup && fadeInCoroutine == null)
        {
            fadeInCoroutine = StartCoroutine(FadeInCutscene(currentCutscene.fadeIn));
        }

        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(true, cutscenePanel);
        }

        DisableOtherAudioSources();

        if (currentCutscene.backgroundMusic)
        {
            StartBackgroundMusic();
        }

        currentClipIndex = 0;
        PlayClip(currentCutscene.clips[currentClipIndex]);
        
        isCutscenePlaying = true;

        if (currentCutscene.canSkipEntireCutscene)
        {
            StartCoroutine(WaitToSkipEntireCutscene());
        }

        return true;
    }
    
    private void EnableInput()
    {
        if (!inputActions)
        {
            Debug.LogError("InputActionAsset not found in CutsceneManager. Please assign the InputActionAsset to the CutsceneManager.");
            return;
        }
        
        inputActions.FindActionMap("UI")?.Enable();
        inputActions.FindActionMap("UI")?.FindAction("Navigate")?.Disable();
        inputActions.FindActionMap("Player")?.Disable();

        if (EventSystem.current.currentSelectedGameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private IEnumerator FadeInCutscene(bool fadeIn = true)
    {
        // Set to black to avoid white flashing
        primaryBackgroundImage.texture = null;
        primaryBackgroundImage.color = Color.black;
        
        secondaryBackgroundImage.texture = null;
        secondaryBackgroundImage.color = Color.black;
        
        if (!fadeIn)
        {
            cutsceneCanvasGroup.alpha = 1f;
            fadeInCoroutine = null;
            yield break;
        }
        
        float timer = 0.0f;

        while (timer < fadeDuration)
        {
            cutsceneCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        
        cutsceneCanvasGroup.alpha = 1f;
        fadeInCoroutine = null;
    }

    private void DisableOtherAudioSources()
    {
        // Refresh AudioSource List
        otherAudioSources.Clear();
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource audioSource in audioSources)
        {
            if (!audioSource.gameObject.GetComponent<CutsceneManager>())
            {
                otherAudioSources.Add(audioSource, audioSource.volume);
            }
        }

        if (fadeOtherAudioCoroutine == null)
        {
            fadeOtherAudioCoroutine = StartCoroutine(FadeOtherSourceVolumes(false));
        }
    }

    private IEnumerator FadeOtherSourceVolumes(bool volumeOn)
    {
        float timer = 0.0f;
        
        // Fade each audiosource volume on or off depending on volumeOn parameter
        while (timer < fadeDuration)
        {
            foreach (var kvp in otherAudioSources)
            {
                AudioSource audioSource = kvp.Key;
                float originalVolume = kvp.Value;
            
                audioSource.volume = Mathf.Lerp(volumeOn ? 0.0f : originalVolume, volumeOn ? originalVolume : 0.0f, timer / fadeDuration);
            }
            
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Finish Transition
        foreach (var kvp in otherAudioSources)
        {
            AudioSource audioSource = kvp.Key;
            float originalVolume = kvp.Value;

            audioSource.volume = volumeOn ? originalVolume : 0.0f;
        }
        
        // Release references if fading back on
        if (volumeOn)
        {
            otherAudioSources.Clear();
        }
        
        fadeOtherAudioCoroutine = null;
    }

    private void StartBackgroundMusic()
    {
        backgroundAudioSource.clip = currentCutscene.backgroundMusic;
        backgroundAudioSource.loop = true;
        backgroundAudioSource.volume = currentCutscene.musicVolume;
        backgroundAudioSource.Play();
    }

    private IEnumerator WaitToSkipEntireCutscene()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        
        canSkipEntireCutscene = true;
        
        holdToSkipPanel.SetActive(true);
    }

    private void PlayClip(CutsceneClip clip)
    {
        // Update background image sprite
        if (transitionBackgroundCoroutine == null)
        {
            transitionBackgroundCoroutine = StartCoroutine(TransitionToNewBackground(clip));
        }
        
        // Play sound effect if clip has one assigned
        if (soundEffectAudioSource && clip.clipSoundEffect)
        {
            soundEffectAudioSource.PlayOneShot(clip.clipSoundEffect, clip.soundEffectVolume);
        }
        
        // Play appropriate Dialogue
        PlayClipDialogue(clip.dialogueLine);
        
        // Wait to allow skipping / auto continuing of the clip
        currentClipCoroutine = StartCoroutine(WaitForClipDuration());
    }

    private IEnumerator TransitionToNewBackground(CutsceneClip clip)
    {
        float timer = 0.0f;
        
        if (clip.backgroundType == CutsceneClip.BackgroundType.SolidColor)
        {
            // Initialization
            Color transparentColor =  new Color(clip.solidColor.r, clip.solidColor.g, clip.solidColor.b, 0f);
            secondaryBackgroundImage.enabled = true;
            secondaryBackgroundImage.texture = null;
            secondaryBackgroundImage.color = transparentColor;
            
            // Fade in new background image on top of old background image
            while (timer < transitionDuration)
            {
                secondaryBackgroundImage.color = Color.Lerp(transparentColor, clip.solidColor, timer / transitionDuration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            
            // Finish fading in
            secondaryBackgroundImage.color = clip.solidColor;
            
            // Update primary image to match instantly
            primaryBackgroundImage.texture = null;
            primaryBackgroundImage.color = clip.solidColor;
            
            // Disable and reset secondary image to use again in next transition
            secondaryBackgroundImage.enabled = false;
            secondaryBackgroundImage.texture = null;
            secondaryBackgroundImage.color = Color.clear;
        }
        else if (clip.backgroundType == CutsceneClip.BackgroundType.Image && clip.backgroundImage)
        {
            // Initialization
            Color transparentColor = new Color(1f, 1f, 1f, 0f);
            secondaryBackgroundImage.enabled = true;
            secondaryBackgroundImage.texture = clip.backgroundImage;
            secondaryBackgroundImage.color = transparentColor;
            
            // Fade in new background image on top of old background image
            while (timer < transitionDuration)
            {
                secondaryBackgroundImage.color = Color.Lerp(transparentColor, Color.white, timer / transitionDuration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            
            // Finish fading in
            secondaryBackgroundImage.color = Color.white;
            
            // Update primary image to match instantly
            primaryBackgroundImage.texture = clip.backgroundImage;
            primaryBackgroundImage.color = Color.white;
            
            // Disable and reset secondary image to use again in next transition
            secondaryBackgroundImage.enabled = false;
            secondaryBackgroundImage.texture= null;
            secondaryBackgroundImage.color = Color.white;
        }
        else if (clip.backgroundType == CutsceneClip.BackgroundType.ImageFromPreviousClip)
        {
            // Do nothing, leave previous image or color on screen.
            Debug.Log("Cutscene clip using image from previous clip.");
        }
        else
        {
            Debug.LogError("No background image assigned to this clip, and useSolidColor is not set to true, cannot transition to next cutscene background image. Please either enable useSolidColor in the inspector of this cutscene clip or assign a background image.");
        }
        
        transitionBackgroundCoroutine = null;
    }

    private void PlayClipDialogue(CutsceneDialogueLine dialogueLine)
    {
        if (isTyping)
        {
            Debug.LogWarning("Dialogue is already typing, ignoring duplicate call");
            return;
        }
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        isTyping = false;
        
        dialogueText.text = "";
        
        if (dialogueLine.Speaker == "")
        {
            speakerNameBackgroundImage.gameObject.SetActive(false);
        }
        else
        {
            speakerNameBackgroundImage.gameObject.SetActive(true);
        }
        
        speakerNameText.text = dialogueLine.Speaker;
        
        SetVoiceGender(dialogueLine.NPCGender);
        
        // Hide continue button if not already hidden
        if (continueButton.gameObject.activeSelf)
        {
            HideContinueButton();
        }

        // type the current line
        typingCoroutine = StartCoroutine(TypeLine(dialogueLine.text));
    }

    // function that handles the typing of the dialogue line
    IEnumerator TypeLine(string line)
    {
        isTyping = true;

        // find intended sound for each letter in current line
        foreach (char c in line)
        {
            dialogueText.text += c;
            PlayTypingSound(c);

            // create a small delay for punctuation
            float delay = .035f;

            switch (c)
            {
                case '.':
                case '!':
                case '?':
                    delay += 0.25f;
                    break;

                case ',':
                case ';':
                case ':':
                    delay += 0.12f;
                    break;
            }

            yield return new WaitForSecondsRealtime(delay);
        }

        isTyping = false;
    }

    private void SetVoiceGender(CutsceneDialogueGender gender)
    {
        if (!dialogueAudioSource) return;
        
        switch (gender)
        {
            case CutsceneDialogueGender.Male:
                dialogueAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("MaleVoice2")[0];
                break;
            
            case CutsceneDialogueGender.Female:
                dialogueAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("FemaleVoice2")[0];
                break;
            
            case CutsceneDialogueGender.NonBinary:
            default:
                dialogueAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("Dialogue")[0];
                break;
        }
    }

    private void PlayTypingSound(char c)
    {
        if (char.IsWhiteSpace(c)) return;

        char up = char.ToUpper(c);
        if (letterSounds.ContainsKey(up))
        {
            dialogueAudioSource.PlayOneShot(letterSounds[up], 0.7f);
        }
    }

    private IEnumerator WaitForClipDuration()
    {
        // Early skip allowing if canSkip overrides duration
        yield return new WaitForSecondsRealtime(0.5f);

        if (currentCutscene.clips[currentClipIndex].canSkipClipEarly)
        {
            ShowContinueButton();
        }
        
        // Wait rest of the duration before either auto continuing or showing the continue button
        yield return new WaitForSecondsRealtime(currentCutscene.clips[currentClipIndex].duration - 0.5f);

        if (currentCutscene.clips[currentClipIndex].autoContinue)
        {
            SkipCurrentClip();
        }
        else
        {
            ShowContinueButton();
        }
    }
    
    private void ShowContinueButton()
    {
        canSkipClip = true;
        continueButton.gameObject.SetActive(true);
    }

    private void HideContinueButton()
    {
        continueButton.gameObject.SetActive(false);
        canSkipClip = false;
    }
    
    private void SkipCurrentClip()
    {
        if (currentClipCoroutine != null)
        {
            StopCoroutine(currentClipCoroutine);
        }
        
        HideContinueButton();
        
        // Increment index to auto start next clip
        currentClipIndex++;
        
        // Return early if it is the last clip to play
        if (currentCutscene.clips.Length <= currentClipIndex && isCutscenePlaying)
        {
            EndCutscene();
            return;
        }
        
        // Play next clip
        PlayClip(currentCutscene.clips[currentClipIndex]);
    }
    
    private void EndBackgroundMusic()
    {
        backgroundAudioSource.Stop();
    }
    
    private void EndCutscene()
    {
        if (cutsceneCanvasGroup && fadeOutCoroutine == null)
        {
            fadeOutCoroutine = StartCoroutine(FadeOutCutscene());
        }
        
        EndBackgroundMusic();
        
        EnableOtherAudioSources();
        
        isCutscenePlaying = false;

        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.SetUIActive(false, null);
        }
        
        Time.timeScale = 1.0f; // Unpause Game
        
        // Invoke event on completion
        currentCutscene.onCutsceneCompleted?.Invoke();
        currentClipIndex = 0;
        currentCutscene = null;
    }
    
    private IEnumerator FadeOutCutscene()
    {
        float timer = 0.0f;
        float duration = 0.25f;

        while (timer < duration)
        {
            cutsceneCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        
        cutsceneCanvasGroup.alpha = 0f;
        
        primaryBackgroundImage.texture = null;
        secondaryBackgroundImage.texture = null;
        
        cutscenePanel.SetActive(false);
        DisableInput();
        
        fadeOutCoroutine = null;
    }

    private void DisableInput()
    {
        if (!inputActions)
        {
            Debug.LogError("InputActionAsset not found in CutsceneManager. Please assign the InputActionAsset to the CutsceneManager.");
            return;
        }
        
        inputActions.FindActionMap("UI")?.Disable();
        inputActions.FindActionMap("UI")?.FindAction("Navigate")?.Enable();
        inputActions.FindActionMap("Player")?.Enable();
    }

    private void EnableOtherAudioSources()
    {
        if (fadeOtherAudioCoroutine == null)
        {
            fadeOtherAudioCoroutine = StartCoroutine(FadeOtherSourceVolumes(true));
        }
    }

    private void OnDisable()
    {
        if (InputDeviceManager.Instance)
        {
            InputDeviceManager.Instance.OnInputModeChanged -= OnInputModeChanged;
        }
    }
}
