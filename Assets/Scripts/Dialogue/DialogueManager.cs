using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // UI
    public GameObject DialoguePanel;
    public TextMeshProUGUI NPCNameText;
    public TextMeshProUGUI DialogueText;
    public Transform ChoicesContainer;
    public GameObject ChoiceButton;
    public GameObject ContinueArrow;
    public GameObject DirectionalImage;
    public ScrollRect dialogueScrollRect;
    private List<GameObject> spawnedChoices = new List<GameObject>();
    private DialogueData currentDialogue;
    private int currentIndex = 0;
    private bool IsTyping;

    // Player Portrait
    public Image playerPortrait;
    public Sprite defaultPortrait;
    public Sprite positivePortrait;
    public Sprite negativePortrait;
    public Sprite neutralPortrait;
    public float portraitFadeDuration = 0.25f;
    public float portraitHoldTime = 0.75f;

    private Coroutine portraitRoutine;
    private CanvasGroup portraitCanvasGroup;

    // Letter sounds
    public AudioSource TypingAudioSource;
    public List<AudioClip> letterClips;
    private Dictionary<char, AudioClip> letterSounds;

    // Navigation sounds
    public AudioSource uiAudioSource;
    // public AudioClip moveClip;
    // public AudioClip confirmClip;

    // Input
    private PlayerControls controls;
    private float MoveInput;
    private bool MoveUpPressed;
    private bool MoveDownPressed;
    private bool ConfirmPressed;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;

    // Selection
    private int SelectedChoiceIndex = 0;
    private bool CanChoose = false;
    public TextMeshProUGUI PopupText;
    public List<HoldDirectionVisual> holdVisuals;
    private Dictionary<ChoiceDirection, HoldDirectionVisual> holdVisualMap;
    public float choiceDistance = 250f;
    private Dictionary<ChoiceDirection, DialogueChoice> directionalChoices = new();
    public float holdTimeToSelect = 1.2f;
    private float directionHoldTimer = 0f;
    private ChoiceDirection? currentHeldDirection;
    private bool isHoldingDirection = false;
    private bool waitingForHoldCompletion = false;
    private bool suppressInputOneFrame = false;

    // Player choice tendencies
    private int positiveChoiceCount = 0;
    private int negativeChoiceCount = 0;
    private int neutralChoiceCount = 0;

    // Typing
    private Coroutine typeingRoutine;
    private string currentFullLine = "";

    // Random choice timer
    public float choiceTimeLimit = 15f;
    public Slider ChoiceTimeSlider;
    private float choiceTimer;
    private Coroutine choiceTimerRoutine;
    //public TextMeshProUGUI TimerText;

    // Player references
    private Transform playerTransform;
    private PlayerThrowing playerThrowing;
    private PlayerFloating playerFloating;
    private PlayerController playerController;
    private CameraMovement cameraMovement;

    // NPC references
    private DialogueTrigger activeDialogueTrigger;
    private Irene ireneNPC;
    private Barry barryNPC;
    private DarryNeighborhood darryNPC;

    public Transform barryDestinationTransform;
    public Transform darryDestinationTransform;
    public Transform ireneDestinationTransform;
    public GameObject IntruderTrigger;

    public static bool DialogueIsActive = false;

    // Player morality
    public int playerMorality = 0;

    private string NPCName;
    private Dictionary<string, int> dialogueVariables = new Dictionary<string, int>();

    private void Awake()
    {
        holdVisualMap = new Dictionary<ChoiceDirection, HoldDirectionVisual>();
        controls = new PlayerControls();
        controls.Dialogue.Move.performed += ctx =>
        {
            Vector2 move = ctx.ReadValue<Vector2>();
            MoveUpPressed = move.y > 0;
            MoveDownPressed = move.y < 0;
        };
        controls.Dialogue.Move.canceled += ctx =>
        {
            //MoveInput = 0f;
            MoveUpPressed = false;
            MoveDownPressed = false;
        };

        foreach (var visual in holdVisuals)
        {
            if (!holdVisualMap.ContainsKey(visual.direction))
            {
                holdVisualMap.Add(visual.direction, visual);
                visual.image.gameObject.SetActive(false);
            }
        }

        controls.Dialogue.Confirm.performed += ctx => ConfirmPressed = true;
        controls.Dialogue.Confirm.canceled += ctx => ConfirmPressed = false;
    }

    private void OnEnable()
    {
        controls.Enable();
        ResetHoldUI();
    }
    private void OnDisable() => controls.Disable();

    void Start()
    {
        DialoguePanel.SetActive(false);
        playerMorality = 0;
        PlayerPrefs.SetInt("Morality", playerMorality);
        PlayerPrefs.Save();

        //playerMorality = PlayerPrefs.GetInt("Morality", 0);
        if (IntruderTrigger != null)
        {
            IntruderTrigger.SetActive(false);
        }

        // Build letter sound dictionary
        letterSounds = new Dictionary<char, AudioClip>();
        for (int i = 0; i < letterClips.Count && i < 26; i++)
        {
            char letter = (char)('A' + i);
            letterSounds[letter] = letterClips[i];
        }

        if (playerPortrait != null)
        {
            portraitCanvasGroup = playerPortrait.GetComponent<CanvasGroup>();

            if (portraitCanvasGroup == null) portraitCanvasGroup = playerPortrait.gameObject.AddComponent<CanvasGroup>();

            playerPortrait.sprite = defaultPortrait;
            portraitCanvasGroup.alpha = 1f;
        }
    }

    void Update()
    {
        HandleChoiceInput();

        if (DialogueIsActive)
        {
            playerController.staminaSlider.gameObject.SetActive(false);
        }

    }

    // -------------------- JSON Dialogue --------------------
    public void StartDialogueFromJson(TextAsset jsonFile, DialogueTrigger trigger)
    {
        cameraMovement = Camera.main.GetComponent<CameraMovement>();

        Debug.Log("DialogueManager: StartDialogueFromJson called");

        cameraMovement.SetCameraLocked(true);

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("DialogueManager GameObject is DISABLED!");
            return;
        }

        if (DialoguePanel == null)
        {
            Debug.LogError("DialogueManager: DialoguePanel is NULL!");
            return;
        }

        activeDialogueTrigger = trigger;
        DialogueIsActive = true;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        ireneNPC = FindAnyObjectByType<Irene>();
        barryNPC = FindAnyObjectByType<Barry>();
        darryNPC = FindAnyObjectByType<DarryNeighborhood>();
        playerTransform = player.transform;
        playerFloating = player.GetComponent<PlayerFloating>();
        playerThrowing = player.GetComponent<PlayerThrowing>();
        playerController = player.GetComponent<PlayerController>();
        PopupText.gameObject.SetActive(false);
        Chime.isInDialogue = true;

        if (darryNPC == null)
        {
            Debug.LogWarning("DarryNeighborhood NPC not found in the scene!");
        }

        if (jsonFile == null)
        {
            Debug.LogWarning("no json dialogue file assigned");
            return;
        }

        currentDialogue = JsonUtility.FromJson<DialogueData>(jsonFile.text);
        currentIndex = 0;

        if (currentDialogue == null)
        {
            Debug.LogError("DialogueData is NULL!");
        }
        else if (currentDialogue.dialogueLines == null)
        {
            Debug.LogError("Dialogue lines are NULL! Check JSON field names");
        }
        else
        {
            Debug.Log($"Dialogue parsed successfully. Lines: {currentDialogue.dialogueLines.Count}");
        }

        if (currentDialogue != null && currentDialogue.dialogueLines != null)
        {
            foreach ( var line in currentDialogue.dialogueLines)
            {
                if (line.choices != null)
                {
                    if (line.choices != null)
                    {
                        foreach (var choice in line.choices)
                        {
                            choice.ParseDirection();
                        }
                    }
                }
            }
        }

        if (currentDialogue == null || currentDialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning("Dialogue JSON invalid or empty");
            return;
        }

        DialoguePanel.SetActive(true);
        NPCNameText.text = currentDialogue.npcName;
        playerController.SetDialogueActive(true);
        if (trigger.focusCameraOnTrigger)
        {
            cameraMovement.TriggerDialogueCamera(trigger.transform);
        }

        if (playerFloating != null) playerFloating.enabled = false;
        if (playerThrowing != null) playerThrowing.enabled = false;

        ShowCurrentLine();

    }

    private void ShowCurrentLine()
    {

        if (dialogueScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            dialogueScrollRect.verticalNormalizedPosition = 1f;
        }

        if (currentIndex >= currentDialogue.dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogue.dialogueLines[currentIndex];
        NPCNameText.text = line.speaker;

        if (line.requiredMorality != 0)
        {
            if((line.requiredMorality > 0 && playerMorality < line.requiredMorality)|| (line.requiredMorality < 0 && playerMorality > line.requiredMorality))
            {
                currentIndex++;
                ShowCurrentLine();
                return;
            }
        }

        if (typeingRoutine != null)
        {
            StopCoroutine(typeingRoutine);
        }

        typeingRoutine = StartCoroutine(TypeLine(line.text));

        foreach (var b in spawnedChoices)
        {
            Destroy(b);
        }
        spawnedChoices.Clear();

        ContinueArrow.SetActive(false);

        // hide slider durring NPC talking
        if (ChoiceTimeSlider != null)
        {
            ChoiceTimeSlider.gameObject.SetActive(false);
        }

    }

    // apply updates to NPC face portraits
   /* private void UpdateSpeakerVisuals(string speaker)
    {
        switch (speaker)
        {
            case "Reed":
                NPCPortrait.sprite = defaultNPCPortrait;
                break;

            case "Darry":
                // darry portrait
                break;
        }
    }*/

    private IEnumerator TypeLine(string text)
    {
        IsTyping = true;
        DialogueText.text = "";
        currentFullLine = text;

        int soundCounter = 0;
        int soundInterval = 2;

        string[] words = text.Split(' ');

        foreach (char c in text)
        {
            // Finish line instantly if confirm pressed
            if (ConfirmPressed && IsTyping)
            {
                DialogueText.text = currentFullLine;
                ConfirmPressed = false;
                IsTyping = false;
                break;
            }

            DialogueText.text += c;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueScrollRect.content);

            if (dialogueScrollRect != null && IsTyping)
            {
                dialogueScrollRect.verticalNormalizedPosition = 1f;
            }

            // Skip sounds for spaces
            if (!char.IsWhiteSpace(c))
            {
                soundCounter++;

                char lookupChar = char.ToUpper(c);

                if (soundCounter % soundInterval == 0 && letterSounds.ContainsKey(lookupChar))
                {
                    TypingAudioSource.PlayOneShot(letterSounds[lookupChar], 0.7f);
                }
            }

            float baseDelay = 0.035f;
            float randomOffset = Random.Range(-0.02f, 0.02f);
            yield return new WaitForSeconds(baseDelay + randomOffset);

            float punctuationPause = GetPauseForCharacter(c);
            if (punctuationPause > 0f)
            {
                yield return new WaitForSeconds(punctuationPause);
            }
        }

        float GetPauseForCharacter(char c)
        {
            switch (c)
            {
                case '.':
                case '!':
                case '?':
                    return 0.25f;

                case ',':
                case ';':
                case ':':
                    return 0.12f;

                default:
                    return 0f;
            }
        }

        IsTyping = false;

        // show continue arrow after the line is typed and there are no choices to be selected
        var choices = currentDialogue.dialogueLines[currentIndex].choices;
        if (choices == null || choices.Count == 0)
        {
            ContinueArrow.SetActive(true);
        }

        // Show choices if any
        if (choices != null && choices.Count > 0)
        {
            SpawnChoices(choices);
        }
        else
        {
            StartCoroutine(WaitForNextLine());
        }

        Canvas.ForceUpdateCanvases();

        if (dialogueScrollRect != null)
        {
            dialogueScrollRect.verticalNormalizedPosition = 1f;
        }

        ConfirmPressed = false;
    }

    private IEnumerator WaitForNextLine()
    {
        if (IsTyping) yield break;

        // Wait for player confirm to advance
        while (ConfirmPressed)
        {
            yield return null;
        }
        
        while (!ConfirmPressed)
        {
            yield return null;
        }
        ConfirmPressed = false;

        DialogueLine line = currentDialogue.dialogueLines[currentIndex];

        // End dialogue if the line says to
        if (line.endDialogueAfterLine)
        {
            if (activeDialogueTrigger.NPCName == "Irene"  && ireneNPC != null)
            {
                if (!ireneNPC.IsFollowing)
                {
                    ireneNPC.IsFollowing = true;
                }
            }

            if (activeDialogueTrigger.NPCName == "IreneStory")
            {
                ireneNPC.StartTravel();
                ireneNPC.dialogueTrigger.TalkedAlready = true;
            }
            /* {
                 //ireneNPC.StartTravel();
                 ireneNPC.IsFollowing = true;
             }
             else if(ireneNPC != null && ireneNPC.IsFollowing == true)
             {
                 ireneNPC.StartTravel();
                 ireneNPC.dialogueTrigger.TalkedAlready = true;
             }*/

            if (ireneNPC != null && activeDialogueTrigger.NPCName == "Irene" && activeDialogueTrigger.TalkedAlready && ireneDestinationTransform != null)
            {
                ireneNPC.targetSpot = ireneDestinationTransform;
                ireneNPC.StartTravel();
            }

            // Move Barry if assigned
             if (barryNPC != null && (activeDialogueTrigger.NPCName == "Reed" || activeDialogueTrigger.NPCName == "Darry") && activeDialogueTrigger.TalkedAlready)
            {
                barryNPC.StartTravel();
            }
            else
            {
                Debug.LogWarning("Barry will not move");
            }

            // Move Darry in Neighborhood and spawn enemy trigger
            if (darryNPC != null && (activeDialogueTrigger.NPCName == "Reed" || activeDialogueTrigger.NPCName == "Darry"))
            {
                if(darryNPC != null)
                {
                    darryNPC.StartTravel();
                }

                // Intruder trigger spawn
                if (IntruderTrigger != null)
                {
                    IntruderTrigger.SetActive(true);
                }
                else
                {
                    Debug.Log("Spawning is sitll inactive");
                }
            }
            else
            {
                Debug.LogWarning("Darry NPC not found in scene!");
            }

            EndDialogue();
            yield break;
        }

        // move to next line (if any)
        if (currentIndex + 1 < currentDialogue.dialogueLines.Count)
        {
            currentIndex++;
            ShowCurrentLine();
        }
        else if (currentIndex < 0)
        {
            EndDialogue();
        }
        else
        {
            EndDialogue();
        }

    }

    private Vector2 GetPositionForDirection(ChoiceDirection dir)
    {
        float distance = choiceDistance;
        switch(dir)
        {
            case ChoiceDirection.Up: return new Vector2(0, distance);
            case ChoiceDirection.Down: return new Vector2(0, -distance);
            case ChoiceDirection.Left: return new Vector2(-distance, 0);
            case ChoiceDirection.Right: return new Vector2(distance, 0);
            default: return Vector2.zero;
        };
    }

    private void PositionDirectionalCross()
    {
        if (DirectionalImage == null) return;

        RectTransform rt = DirectionalImage.GetComponent<RectTransform>();
        rt.SetParent(ChoicesContainer, false);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

    }

    private void SpawnChoices(List<DialogueChoice> choices)
    {
        // Clear old
        foreach (var b in spawnedChoices) Destroy(b);
        spawnedChoices.Clear();
        directionalChoices.Clear();
        ChoicesContainer.gameObject.SetActive(true);

        // Spawn direction cross
        if (DirectionalImage != null)
        {
            DirectionalImage.SetActive(true);
            PositionDirectionalCross();
        }

        foreach (DialogueChoice choice in choices)
        {
            GameObject buttonObj = Instantiate(ChoiceButton, ChoicesContainer);
            buttonObj.SetActive(true);
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.text;

            RectTransform rt = buttonObj.GetComponent<RectTransform>();
            rt.localScale = Vector3.one * 3f;
            rt.anchoredPosition = GetPositionForDirection(choice.direction);


           // Button btn = buttonObj.GetComponent<Button>();
           // int next = choice.nextIndex;
           // btn.onClick.AddListener(() => OnChoiceSelected(choice));
            spawnedChoices.Add(buttonObj);
            directionalChoices[choice.direction] = choice;

            Debug.Log($"Choice '{choice.text}' spawned at {rt.anchoredPosition} for direction {choice.direction}");
        }

        CanChoose = true;
        SelectedChoiceIndex = 0;

        // helps stop auto selection
        suppressInputOneFrame = true;
        StartCoroutine(ReleaseInputNextFrame());
        //UpdateChoiceHighlight();

        // activate slider
        if (ChoiceTimeSlider != null)
        {
            ChoiceTimeSlider.gameObject.SetActive(true);
            ChoiceTimeSlider.value = 1f;
        }

        // Start timer countdown for auto-select
        if (choiceTimerRoutine != null)
        {
            StopCoroutine(choiceTimerRoutine);
        }
        choiceTimerRoutine = StartCoroutine(ChoiceTimerCountdown(choices));

    }

    private IEnumerator ReleaseInputNextFrame()
    {
        yield return null;
        suppressInputOneFrame = false;
    }

    private void HandleChoiceInput()
    {
        if (suppressInputOneFrame) return;

        if ((!CanChoose || spawnedChoices.Count == 0))
        {
            ResetHoldUI();
            return;
        }

        Vector2 input = controls.Dialogue.Move.ReadValue<Vector2>();

        const float deadzone = 0.4f;

        // if released cancel circle
        if (input.sqrMagnitude < deadzone * deadzone)
        {
            ResetHoldUI();
            return;
        }

        if (!isHoldingDirection)
        {
            ChoiceDirection newDir = GetDirectionFromInput(input);

            //if (currentHeldDirection.HasValue && currentHeldDirection.Value == newDir) return;

            if (!directionalChoices.ContainsKey(newDir))
            {
                ResetHoldUI();
                return;
            }

            currentHeldDirection = newDir;
            directionHoldTimer = 0f;
            isHoldingDirection = true;
        }

        // check
        if (currentHeldDirection == null)
        {
            return;
        }

        directionHoldTimer += Time.deltaTime;

        ChoiceDirection dir = currentHeldDirection.Value;

        HighlightDirection(dir);
        
        UpdateHoldUI(directionHoldTimer / holdTimeToSelect);
        
        if (directionHoldTimer >= holdTimeToSelect)
        {
            CompleteHold(dir);
        }
    }

    // Change echo portrait based on answer selection
    private IEnumerator SwapPortrait(Sprite newPortrait)
    {
        if (playerPortrait == null || portraitCanvasGroup == null) yield break;

        // fade out
        float t = 0f;
        while (t < portraitFadeDuration)
        {
            t += Time.deltaTime;
            portraitCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / portraitFadeDuration);
            yield return null;
        }

        // swap image
        playerPortrait.sprite = newPortrait;

        // fade in
        t = 0f;
        while (t < portraitFadeDuration)
        {
            t += Time.deltaTime;
            portraitCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / portraitFadeDuration);
            yield return null;
        }

        // hold 
        yield return new WaitForSeconds(portraitHoldTime);

        // fade back to default
        t = 0f;
        while (t < portraitFadeDuration)
        {
            t += Time.deltaTime;
            portraitCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / portraitFadeDuration);
            yield return null;
        }

        playerPortrait.sprite = defaultPortrait;

        t = 0f;
        while (t < portraitFadeDuration)
        {
            t += Time.deltaTime;
            portraitCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / portraitFadeDuration);
            yield return null;
        }
    }

    private void UpdateHoldUI(float progress)
    {
        if (currentHeldDirection == null) 
        {
            return;
        }
        
        if (!holdVisualMap.TryGetValue(currentHeldDirection.Value, out var visual))
        {
            return;
        }

        if (!visual.image.gameObject.activeSelf)
        {
            visual.image.gameObject.SetActive(true);
        }

        visual.image.fillAmount = Mathf.Clamp01(progress);
    }

    private void ResetHoldUI()
    {
        bool releasedDuringGrace = waitingForHoldCompletion;
        
        directionHoldTimer = 0f;
        isHoldingDirection = false;
        currentHeldDirection = null;

        foreach (var visual in holdVisualMap.Values)
        {
            visual.image.fillAmount = 0f;
            visual.image.gameObject.SetActive(false);
        }

        // Player released after timer expired
        if (releasedDuringGrace && CanChoose)
        {
            waitingForHoldCompletion = false;
            AutoSelectFallback(new List<DialogueChoice>(directionalChoices.Values));
        }
    }

    private void CompleteHold(ChoiceDirection dir)
    {
        if (!CanChoose) return;

        CanChoose = false;
        SelectDirectionalChoice(dir);
        ResetHoldUI();
    }

    private ChoiceDirection GetDirectionFromInput(Vector2 input)
    {
        if(Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return input.x > 0 ? ChoiceDirection.Right : ChoiceDirection.Left;
        }
        else
        {
            return input.y > 0 ? ChoiceDirection.Up : ChoiceDirection.Down;
        }
    }

    private void SelectDirectionalChoice(ChoiceDirection dir)
    {
        if (!directionalChoices.ContainsKey(dir))
        {
            return;
        }

        OnChoiceSelected(directionalChoices[dir]);
    }

    private void HighlightDirection(ChoiceDirection dir)
    {
        // base color of text is white
        foreach (var obj in spawnedChoices)
        {
            obj.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }

        if (!directionalChoices.ContainsKey(dir)) return;

        DialogueChoice selectedChoice = directionalChoices[dir];

        int selectedIndex = spawnedChoices.FindIndex(o =>
            o.GetComponentInChildren<TextMeshProUGUI>().text == selectedChoice.text);

        if (selectedIndex >= 0)
        {
            var selectedText = spawnedChoices[selectedIndex].GetComponentInChildren<TextMeshProUGUI>();

            // highlight good choice to green
            if (selectedChoice.moralityChange > 0)
            {
                selectedText.color = Color.green;
            }
            // highlight bad choice to red
            else if (selectedChoice.moralityChange < 0)
            {
                selectedText.color = Color.red;
            }
            // highlight nuetral choice to yellow
            else
            {
                selectedText.color = Color.yellow;
            }

            if (holdVisualMap.TryGetValue(dir, out var visual))
            {
                RectTransform choiceRT = spawnedChoices[selectedIndex].GetComponent<RectTransform>();
                RectTransform visualRT = visual.image.GetComponent<RectTransform>();
            }
        }
    }

    private void OnChoiceSelected(DialogueChoice chosen)
    {
        // Hard reset
        isHoldingDirection = false;
        waitingForHoldCompletion = false;
        currentHeldDirection = null;
        directionHoldTimer = 0f;

        /*if (holdCircleImage != null)
        {
            holdCircleImage.fillAmount = 0f;
            holdCircleImage.gameObject.SetActive(false);
        }*/

        // prevent confirm from instantly skipping the next line
        ConfirmPressed = false;

        if (choiceTimerRoutine != null)
        {
            StopCoroutine(choiceTimerRoutine);
            choiceTimerRoutine = null;
        }

        if (DirectionalImage != null)
        {
            DirectionalImage.SetActive(false);
        }

        CanChoose = false;

        // Apply variable change if any
        playerMorality += chosen.moralityChange;
        PlayerPrefs.SetInt("Morality", playerMorality);
        PlayerPrefs.Save();

        Debug.Log($"Morality changed by {chosen.moralityChange}. New Morality: {playerMorality}");
 
        // increase player choice tendencies
        if (chosen.moralityChange > 0)
        {
            positiveChoiceCount++;
        }
        if (chosen.moralityChange < 0)
        {
            negativeChoiceCount++;
        }
        else
        {
            neutralChoiceCount++;
        }

        // change portrait based on morality
        if (portraitRoutine != null)
        {
            StopCoroutine(portraitRoutine);
        }

        Sprite portraitToUse =
                chosen.moralityChange > 0 ? positivePortrait :
                chosen.moralityChange < 0 ? negativePortrait :
                neutralPortrait;

        portraitRoutine = StartCoroutine(SwapPortrait(portraitToUse));

        // clear old choices
        foreach (var b in spawnedChoices) Destroy(b);
        spawnedChoices.Clear();
        ChoicesContainer.gameObject.SetActive(false);

        // show pop up of morality change
        if (chosen.moralityChange != 0)
        {
            ShowPopUp($"Morality changed by {chosen.moralityChange}. New Morality: {playerMorality}", 2f);
        }

        // Trigger an objective if this choice has one
        if (!string.IsNullOrEmpty(chosen.objectiveToActivate))
        {
            ObjectiveManager.Instance.ActivateObjectiveByID(chosen.objectiveToActivate);
        }

        // Continue dialogue
        if (chosen.nextIndex >= 0 && chosen.nextIndex < currentDialogue.dialogueLines.Count)
        {
            currentIndex = chosen.nextIndex;
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }

        /*if (TimerText != null)
        {
            TimerText.text = "";
        }*/

        // hide slider agian when choice is selected
        if (ChoiceTimeSlider != null)
        {
            ChoiceTimeSlider.gameObject.SetActive(false);
        }

    }

    private DialogueChoice GetBiasedChoice(List<DialogueChoice> choices)
    {
        if (choices == null || choices.Count == 0)
            return null;

        // Determine most frequent tendency
        int dominant = Mathf.Max(positiveChoiceCount, negativeChoiceCount, neutralChoiceCount);

        int preferredSign = dominant == positiveChoiceCount ? 1 : dominant == negativeChoiceCount ? -1 : 0;

        DialogueChoice bestChoice = null;
        float bestScore = float.MinValue;

        foreach (var choice in choices)
        {
            float score = 0f;

            // Match plaer tendency
            if (Mathf.Sign(choice.moralityChange) == preferredSign)
            {
                score += 3f;
            }

            // soft bias toward previously similar magnitude
            score -= Mathf.Abs(choice.moralityChange - playerMorality * 0.1f);

            // small randomness
            score += Random.Range(0f, 0.5f);

            if (score > bestScore)
            {
                bestScore = score;
                bestChoice = choice;
            }

        }

        return bestChoice;
    }

    private IEnumerator ChoiceTimerCountdown(List<DialogueChoice> choices)
    {
        choiceTimer = choiceTimeLimit;

        if (ChoiceTimeSlider != null)
        {
            ChoiceTimeSlider.gameObject.SetActive(true);
            ChoiceTimeSlider.value = 1f;
        }

        while (choiceTimer > 0f && CanChoose)
        {
            choiceTimer -= Time.deltaTime;

            if (ChoiceTimeSlider != null)
            {
                // Normalize time (1 to 0)
                ChoiceTimeSlider.value = choiceTimer / choiceTimeLimit;
            }

            yield return null;
        }

        if (ChoiceTimeSlider != null)
        {
            ChoiceTimeSlider.gameObject.SetActive(false);
        }

        // Timer expired and player didn't choose
        if (CanChoose)
        {
            DialogueChoice fallback = GetBiasedChoice(choices);

            if (isHoldingDirection && currentHeldDirection.HasValue)
            {
                waitingForHoldCompletion = true;
                Debug.Log("Timer expired, but player is holding - waiting for completion");
                yield break;
            }

            // timer ran out and not holding a direction
            OnChoiceSelected(fallback);

           /* if (fallback != null)
            {
                Debug.Log("Timer expired, bias-selecting choice: " + fallback.text);
                OnChoiceSelected(fallback);
            }

            if (fallback == null && choices.Count > 0)
            {
                fallback = choices[0];
            }

            if (fallback != null)
            {
                Debug.Log("Timer expired, auto-selecting choice: " + fallback.text);
                OnChoiceSelected(fallback);
            }*/
        }
    }

    private void AutoSelectFallback(List<DialogueChoice> choices)
    {
        if (!CanChoose) return;

        DialogueChoice fallback = GetBiasedChoice(choices);

        if (fallback == null && choices.Count > 0)
        {
            fallback = choices[0];
        }

        if (fallback != null)
        {
            Debug.Log("Auto-selecting fallback choice: " + fallback.text);
            OnChoiceSelected(fallback);
        }
    }

    public void ShowPopUp(string message, float duration = 1f)
    {
        PopupText.gameObject.SetActive(true);
        PopupText.alpha = 1f;
       // PopupText.transform.localPosition = Vector3.zero;

        StopCoroutine(nameof(ShowPopupRoutine));
        StartCoroutine(ShowPopupRoutine(message, duration));
    }

    private IEnumerator ShowPopupRoutine(string message, float duration)
    {
        //PopupText.gameObject.SetActive(true);
        PopupText.text = message;

        // Capture starting position
       // Vector3 startPos = PopupText.transform.localPosition;
       // Vector3 endPos = startPos + Vector3.up * 20f;

        // Fade out over time
        yield return new WaitForSeconds(duration);

        float fadeDuration = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            PopupText.alpha = Mathf.Lerp(1f, 0f, t);
            //PopupText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            //PopupText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        PopupText.text = "";
        PopupText.alpha = 0f;
        //PopupText.transform.localPosition = startPos;
        PopupText.gameObject.SetActive(false);
    }

    public void EndDialogue()
    {
        if (activeDialogueTrigger != null)
        {
            activeDialogueTrigger.StopLookingAtPlayer();
            activeDialogueTrigger.ResumeWandering();

            if (ButtonIcons.Instance != null)
            {
                ButtonIcons.Instance.Highlight(activeDialogueTrigger.interactType);
            }

            activeDialogueTrigger = null;
        }

        DialoguePanel.SetActive(false);
        currentDialogue = null;
        currentIndex = 0;
        spawnedChoices.Clear();
        ConfirmPressed = false;
        CanChoose = false;
        DialogueIsActive = false;
        ContinueArrow.SetActive(false);
        
        playerController.SetDialogueActive(false);
        StartCoroutine(cameraMovement.EndCameraZoom());

        if (playerFloating != null) playerFloating.enabled = true;
        if (playerThrowing != null) playerThrowing.enabled = true;
        if (TypingAudioSource != null) TypingAudioSource.Stop();

        // Only auto follow if Irene does not have a destination to travel to
        if (ireneNPC != null && ireneNPC.gameObject.activeInHierarchy)
        {
            if (ireneNPC.NPCNameMatches(NPCNameText.text) && ireneNPC.CanFollowPlayer)
            {
                ireneNPC.IsFollowing = true;
            }
        }

        StartCoroutine(EndDialogueSafe());
        Debug.Log($"Dialogue ended. Final morality = {playerMorality}");
    }

    private IEnumerator EndDialogueSafe()
    {
        // Wait one frame so UI Toolkit + jobs finish safely
        yield return null;

        if (cameraMovement != null)
        {
            // If EndCameraZoom is a coroutine
            yield return cameraMovement.EndCameraZoom();
        }

        Chime.isInDialogue = false;

        Debug.Log("EndDialogueSafe: cleanup complete");
    }

    public int GetVariable(string morality)
    {
        if (dialogueVariables.TryGetValue(morality, out int value))
        {
            return value;
        }
        return 0;
    }

    public void SetVariable(string morality, int value)
    {
        dialogueVariables[morality] = value;
    }
}
