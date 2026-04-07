using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image blackScreenImage;
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
    [SerializeField] private Sprite holdKeySprite;
    [SerializeField] private Sprite continueButtonSprite;
    [SerializeField] private Sprite dialogueBackgroundSprite;
    [SerializeField] private Sprite speakerBackgroundSprite;
    
    [Header("Audio")]
    [SerializeField] private AudioMixer mainAudioMixer;
    
    [Tooltip("Audio clips of each letter A-Z")]
    [SerializeField] List<AudioClip> letterClips;
    
    private Dictionary<char, AudioClip> letterSounds = new();
    private AudioSource backgroundAudioSource;
    private AudioSource dialogueAudioSource;
    private AudioSource soundEffectAudioSource;
    
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

    private bool isTyping = false;
    private Coroutine typingCoroutine;
    
    [HideInInspector] public bool isCutscenePlaying = false;

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

    private void InitializeAudioSources()
    {
        var audioSources = GetComponents<AudioSource>();
        
        if (audioSources.Length < 3)
        {
            Debug.LogError("CutsceneManager requires at least 2 AudioSources. Please add more AudioSources to the CutsceneManager GameObject.");
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
    
    private void Update()
    {
        ProcessHoldToSkip();
    }

    private void ProcessHoldToSkip()
    {
        if (!isHoldingSkip) return;
        
        skipTimer += Time.deltaTime;

        if (holdToSkipSlider)
        {
            holdToSkipSlider.value = skipTimer;
        }
        
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

        if (holdToSkipSlider && canSkipEntireCutscene)
        {
            holdToSkipSlider.gameObject.SetActive(true);
            holdToSkipSlider.value = 0.0f;
        }
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
        
        Time.timeScale = 0f; // Pause the game while the cutscene is playing
        
        currentCutscene = cutscene;

        if (!cutscenePanel.activeSelf)
        {
            cutscenePanel.SetActive(true);
        }

        if (currentCutscene.backgroundMusic)
        {
            StartBackgroundMusic();
        }
        
        PlayClip(currentCutscene.clips[currentClipIndex]);
        
        isCutscenePlaying = true;

        if (currentCutscene.canSkipEntireCutscene)
        {
            StartCoroutine(WaitToSkipEntireCutscene());
        }

        return true;
    }

    private void StartBackgroundMusic()
    {
        backgroundAudioSource.clip = currentCutscene.backgroundMusic;
        backgroundAudioSource.loop = true;
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
        backgroundImage.sprite = clip.backgroundImage;
        
        // Play sound effect if clip has one assigned
        if (soundEffectAudioSource && clip.clipSoundEffect)
        {
            soundEffectAudioSource.PlayOneShot(clip.clipSoundEffect);
        }
        
        // Play appropriate Dialogue
        PlayClipDialogue(clip.dialogueLine);
        
        // Wait to allow skipping / auto continuing of the clip
        currentClipCoroutine = StartCoroutine(WaitForClipDuration());
    }

    private void PlayClipDialogue(CutsceneDialogueLine dialogueLine)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        isTyping = false;
        
        dialogueText.text = "";
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

            yield return new WaitForSeconds(delay);
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
        if (currentCutscene.clips.Length <= currentClipIndex)
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
        cutscenePanel.SetActive(false);
        
        EndBackgroundMusic();
        
        backgroundImage.sprite = null;
        
        isCutscenePlaying = false;
        
        Time.timeScale = 1.0f; // Unpause Game
        
        // Invoke event on completion
        currentCutscene.onCutsceneCompleted?.Invoke();
        currentCutscene = null;
    }
}
