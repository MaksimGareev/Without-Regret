using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class NewDialogueManager : MonoBehaviour
{
    public static NewDialogueManager Instance;

    [Header("UI")]
    [Tooltip("Main dialogue panel shown during dialogue interaction")]
    public GameObject dialoguePanel;
    [Tooltip("The display text of the current dialogue line")]
    public TextMeshProUGUI dialogueText;
    [Tooltip("Name display of the speaker")]
    public TextMeshProUGUI npcNameText;
    [Tooltip("The transform containing the dialogue choices")]
    [SerializeField] Transform choiceContainer;
    [Tooltip("The arrow image showing the player which direction to press to select an answer")]
    [SerializeField] GameObject directionalImage;
    [Tooltip("Button prefab made for answer choices")]
    [SerializeField] GameObject choicePrefab;
    [Tooltip("Blinking continue arrow indicating the player can continue to the next line")]
    [SerializeField] GameObject continueArrow;
    [Tooltip("A scroll rect used to scroll through longer instances of dialogue")]
    [SerializeField] ScrollRect scrollRect;
    [Tooltip("Slider indicating the time the player has to make a dialogue choice")]
    [SerializeField] Slider choiceTimerSlider;
    [Tooltip("Pop up text showing the players change in morality and current total morality")]
    [SerializeField] TextMeshProUGUI popupText;
    [Tooltip("The visual feedback of the players dialogue choice input")]
    [SerializeField] List<HoldDirectionVisual> holdVisuals;

    [Header("Player Portrait")]
    [Tooltip("Copy image of the players UI")]
    public Image playerPortrait;
    [Tooltip("Sprites that appear in response to different answer choices")]
    [SerializeField] Sprite defaultPortrait, positivePortrait, negativePortrait, neutralPortrait;
    [Tooltip("How long the transition takes between portraits")]
    [SerializeField] float portraitFadeTime = 0.25f;
    [Tooltip("How long the new portrait stays before reverting back to the default")]
    [SerializeField] float portraitHoldTime = 0.75f;

    [Header("NPC Portraits")]
    [SerializeField] private Image npcPortrait;
    [SerializeField] private List<NPCPortraitSet> npcPortraitSets;

    [Header("Audio")]
    [SerializeField] AudioSource typingSource;
    [Tooltip("Audio clips of each letter A-Z")]
    [SerializeField] List<AudioClip> letterClips;

    [Header("Audio Mixer Groups")]
    [SerializeField] AudioMixerGroup maleVoiceGroup;
    [SerializeField] AudioMixerGroup femaleVoiceGroup;

    [Header("Morality Settings")]
    public int playerMorality;
    [SerializeField] private int minMorality = -10;
    [SerializeField] private int maxMorality = 10;

    [Header("Choice Selection")]
    [Tooltip("How long the player needs to hold to confirm a selection")]
    [SerializeField] float holdTimeToSelect = 1.2f;
    [Tooltip("How much time the player has to make a choice")]
    [SerializeField] float choiceTimeLimit = 15f;
    [Tooltip("How far the answer choices are spaced out from the center of the choice container")]
    [SerializeField] float choiceDistance = 250f;

    [Header("NPC movement (trying to remove")]
    private Irene ireneNPC;
    private Barry penelopeNPC;
    private Barry barryNPC;
    private DarryNeighborhood darryNPC;
    public Transform ireneDestinationTransform;
    public GameObject IntruderTrigger;
  
    private NewDialogueData dialogue;
    private Dictionary<string, NewDialogueLineData> lineLookup = new();
    private NewDialogueLineData currentLine;
    private string currentLineID;

    bool typing;
    bool canChoose;
    bool resolvingChoice;
    bool waitingForHoldCompletion;

    Dictionary<char, AudioClip> letterSounds = new();
    Dictionary<ChoiceDirection, NewDialogueChoiceData> directionalChoices = new();
    Dictionary<ChoiceDirection, HoldDirectionVisual> holdMap = new();
    List<GameObject> spawnedChoices = new();

    float holdTimer;
    ChoiceDirection? currentDir;

    Coroutine typingRoutine;
    Coroutine timerRoutine;
    Coroutine portraitRoutine;

    CanvasGroup portraitGroup;

    int posCount, negCount, neutralCount;

    private NewDialogueTrigger activeDialogueTrigger;
    public bool DialogueIsActive { get; private set; }
    bool justStartedDialogue = false;

    private PlayerController playerController;
    private CameraMovement cam;
    PlayerControls controls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        controls = new PlayerControls();

        portraitGroup = playerPortrait.GetComponent<CanvasGroup>();
        if (!portraitGroup)
        {
            portraitGroup = playerPortrait.gameObject.AddComponent<CanvasGroup>();
        }

        foreach (var v in holdVisuals)
        {
            holdMap[v.direction] = v;
            v.image.fillAmount = 0;
            v.image.gameObject.SetActive(false);
        }

        BuildLetterSounds();
        SetupInput();
        
    }

    void SetupInput()
    {
        controls.Dialogue.Confirm.performed += _ => OnConfirmPressed();
    }

    // Build the dictionary of dialogue letter sounds
    void BuildLetterSounds()
    {
        for (int i = 0; i < letterClips.Count; i++)
        {
            letterSounds[(char)('A' + i)] = letterClips[i];
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        if (!canChoose) return;
        HandleDirectionalSelection();
    }

    // Load dialogue based on the intended Scriptable object dialogue 
    public void StartDialogue(NewDialogueData dialogueSO, NewDialogueTrigger trigger)
    {
        justStartedDialogue = true;
        StartCoroutine(ClearStartFlag());

        // check if scriptable object is present
        if (dialogueSO == null || dialogueSO.dialogueLines.Count == 0)
        {
            Debug.LogError("Dialogue data is empty or missing");
            return;
        }

        activeDialogueTrigger = trigger;
        dialogue = dialogueSO;
        lineLookup.Clear();

        // search for line id to start dialogue
        foreach (var line in dialogue.dialogueLines)
        {
            lineLookup[line.LineID] = line;
        }

        currentLineID = dialogue.dialogueLines[0].LineID;

        playerController = FindAnyObjectByType<PlayerController>();
        cam = Camera.main.GetComponent<CameraMovement>();

        // activate dialogue UI
        dialoguePanel.SetActive(true);
        playerPortrait.gameObject.SetActive(true);
        npcNameText.text = dialogue.npcName;

        DialogueIsActive = true;

        // set player controler to dialogue
        if (playerController != null)
        {
            playerController.SetDialogueActive(true);
        }

        // zoom the camera if focus camera on trigger bool is true
        if (cam != null)
        {
            cam.SetCameraLocked(true);
            if (trigger != null && trigger.focusCameraOnTrigger)
            {
                cam.TriggerDialogueCamera(trigger.transform);
            }
        }

        ShowLine();
    }

    IEnumerator ClearStartFlag()
    {
        yield return null;
        justStartedDialogue = false;
    }

    // Show the current line
    private void ShowLine()
    {
        // If there is no line ID end dialogue
        if (!lineLookup.ContainsKey(currentLineID))
        {
            EndDialogue();
            return;
        }

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }

        typing = false;

        currentLine = lineLookup[currentLineID];
        dialogueText.text = "";
        npcNameText.text = currentLine.Speaker;

        // set portrait and voice of speaker
        SetNPCPortrait(currentLine.lineTone);
        if (activeDialogueTrigger != null && activeDialogueTrigger.faceHandler != null) //Calls the faceHandler to display same expression as the NPC Portrait
        {
            activeDialogueTrigger.faceHandler.SetExpression(currentLine.lineTone);
        }
        SetVoiceGender(currentLine.NPCGender);

        // hide continue arrow and choices
        continueArrow.SetActive(false);
        ClearChoices();

        // type the current line
        typingRoutine = StartCoroutine(TypeLine(currentLine));

    }

    // function that handles the typing of the dialogue line
    IEnumerator TypeLine(NewDialogueLineData line)
    {
        typing = true;
        // hide slider durring typing
        choiceTimerSlider.gameObject.SetActive(false);

        // find intended sound for each letter in current line
        foreach (char c in line.text)
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

        typing = false;

        // if there are no choices present after the line is finished typing have the continue arrow spawn
        if (line.choices == null || line.choices.Count == 0)
        {
            continueArrow.SetActive(true);
        }
        // if choices are available after line is built spawn choices
        else
        {
            SpawnChoices(line.choices);
        }
    }

    void OnConfirmPressed()
    {
        if (justStartedDialogue) return;

        // don't allow player to skip anything durring pauses in dialogue
        if (!DialogueIsActive) return;
        if (currentLine == null) return;
        if (resolvingChoice) return;

        // if line is typing and confirm is pressed have the line be build instantly and spawn arrow if needed
        if (typing)
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
            }
            dialogueText.text = currentLine.text;
            typing = false;

            if (currentLine.choices == null || currentLine.choices.Count == 0)
            {
                continueArrow.SetActive(true);
            }
            else
            {
                SpawnChoices(currentLine.choices);
            }

            return;
        }

        if (canChoose) return;

        // if end dialogue after line is true and player presses confirm end the dialogue
        if (currentLine.endDialogueAfterLine)
        {
            HandleNPCMovementsAfterLine();
            EndDialogue();
            return;
        }

        // go to next line if there is a next line ID intended
        if (!string.IsNullOrEmpty(currentLine.NextLineID))
        {
            currentLineID = currentLine.NextLineID;
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }
    
    // Move NPCs or trigger irene to follow the player
    void HandleNPCMovementsAfterLine()
    {
        if (activeDialogueTrigger == null) return;

        string npcName = activeDialogueTrigger.NPCName;

        Irene irene = FindObjectOfType<Irene>();

        switch (npcName)
        {
            case "Irene":
                if (irene != null)
                {
                    if (!irene.IsFollowing && activeDialogueTrigger.IsMediation == false)
                    {
                        irene.IsFollowing = true;
                        irene.Follow();
                    }
                    
                   /* if (irene.targetSpot != null)
                    {
                        irene.IsFollowing = false;
                        irene.StartTravel();
                    }
                    */

                    if (activeDialogueTrigger.hasTalked && irene.GoBackHomeSpot != null && irene.arrived == true)
                    {
                        irene.targetSpot = irene.GoBackHomeSpot;
                        irene.StartTravel();
                    }
                }
                break;

            case "Irene Story":

                if (irene != null && irene.targetSpot != null)
                {
                    irene.StartTravel();
                }
                break;

            case "Reed":
            case "Darry":
                Barry barry = FindObjectOfType<Barry>();
                if (barry != null)
                {
                    barry.StartTravel();
                }

                DarryNeighborhood darry = FindObjectOfType<DarryNeighborhood>();
                if (darry != null)
                {
                    darry.StartTravel();
                }
                break;

            case "Penelope":
                Barry penelope = FindObjectOfType<Barry>();
                if (penelope != null)
                {
                    penelope.StartTravel();
                }
                break;
            
            case "Echo":
                Barry echo = FindObjectOfType<Barry>();
                if (echo != null)
                {
                    echo.StartTravel();
                }
                break;
        }
    }
    

    // handle spawning of choices when intended
    void SpawnChoices(List<NewDialogueChoiceData> choices)
    {
        canChoose = true;
        directionalChoices.Clear();
        choiceTimerSlider.gameObject.SetActive(true);

        if (activeDialogueTrigger != null)
            activeDialogueTrigger.StartPlayerThinking(); //Call NPC to start playing Player thinking animations while the player is making a choice

        // spawn directional arrow to show which direction to hold to select answer
        if (directionalImage != null)
        {
            directionalImage.SetActive(true);
            RectTransform rt = directionalImage.GetComponent<RectTransform>();
            rt.SetParent(choiceContainer, false);
            rt.anchoredPosition = Vector2.zero;
        }

        // create buttons and text from list of choices
        foreach (var c in choices)
        {
            GameObject obj = Instantiate(choicePrefab, choiceContainer);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = c.text;
            obj.GetComponent<RectTransform>().anchoredPosition = GetDirPos(c.direction);

            spawnedChoices.Add(obj);
            directionalChoices[c.direction] = c;
        }

        // start decreaseing timer value of slider
        timerRoutine = StartCoroutine(ChoiceTimer(choices));
    }

    // calculate morality effect of slected choice and enable pop up
    IEnumerator ResolveChoiceRoutine(NewDialogueChoiceData c)
    {
        ApplyMorality(c.moralityChange);

        ShowPopup($"Morality changed by {c.moralityChange}. New Morality: {playerMorality}");

        yield return new WaitForSeconds(portraitFadeTime * 2 + portraitHoldTime);

        resolvingChoice = false;

        if (!string.IsNullOrEmpty(c.NextLineID))
        {
            currentLineID = c.NextLineID;
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    // Handle countdown of time remaining to select answer choice
    IEnumerator ChoiceTimer(List<NewDialogueChoiceData> choices)
    {
        float t = choiceTimeLimit;

        while (t > 0 && canChoose)
        {
            t -= Time.deltaTime;
            choiceTimerSlider.value = t / choiceTimeLimit;
            yield return null;
        }

        if (!canChoose) yield break;

        if (currentDir != null)
        {
            waitingForHoldCompletion = true;
            yield break;
        }

        SelectChoice(GetBiasedChoice(choices));
    }

    // Handle auto selection if player does not choose in time, take into account past answers with slight randomness
    NewDialogueChoiceData GetBiasedChoice(List<NewDialogueChoiceData> choices)
    {
        int dominant = Mathf.Max(posCount, negCount, neutralCount);
        int sign = dominant == posCount ? 1 : dominant == negCount ? -1 : 0;

        NewDialogueChoiceData best = choices[0];
        float bestScore = float.MinValue;

        foreach (var c in choices)
        {
            float score = Random.value;
            if (Mathf.Sign(c.moralityChange) == sign)
            {
                score += 3f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }
        return best;
    }

    // apply morality change of selected answer choice
    void ApplyMorality(int change)
    {
        playerMorality += change;

        // clamp morality between min and max
        playerMorality = Mathf.Clamp(playerMorality, minMorality, maxMorality);

        PlayerPrefs.SetInt("Morality", playerMorality);

        if (change > 0) posCount++;
        else if (change < 0) negCount++;
        else neutralCount++;

        Sprite newPortrait = change > 0 ? positivePortrait :
                             change < 0 ? negativePortrait :
                             neutralPortrait;

        if (portraitRoutine != null)
        {
            StopCoroutine(portraitRoutine);
        }

        portraitRoutine = StartCoroutine(SwapPortrait(newPortrait));
    }

    // Change player and NPC portrait in response to dialogue choice selection
    private IEnumerator SwapPortrait(Sprite newPortrait)
    {
        if (playerPortrait == null || portraitGroup == null) yield break;
        directionalImage.SetActive(false);
        Debug.Log("swaping portrait");

        // fade out
        float t = 0f;
        while (t < portraitFadeTime)
        {
            t += Time.deltaTime;
            portraitGroup.alpha = Mathf.Lerp(1f, 0f, t / portraitFadeTime);
            yield return null;
        }

        // swap image
        playerPortrait.sprite = newPortrait;

        // fade in
        t = 0f;
        while (t < portraitFadeTime)
        {
            t += Time.deltaTime;
            portraitGroup.alpha = Mathf.Lerp(0f, 1f, t / portraitFadeTime);
            yield return null;
        }

        // hold
        yield return new WaitForSeconds(portraitHoldTime);

        // fade back to default
        t = 0f;
        while (t < portraitFadeTime)
        {
            t += Time.deltaTime;
            portraitGroup.alpha = Mathf.Lerp(1f, 0f, t / portraitFadeTime);
            yield return null;
        }

        playerPortrait.sprite = defaultPortrait;

        t = 0f;
        while (t < portraitFadeTime)
        {
            t += Time.deltaTime;
            portraitGroup.alpha = Mathf.Lerp(0f, 1f, t / portraitFadeTime);
            yield return null;
        }
    }

    // read the value of directional input when choices are present
    void HandleDirectionalSelection()
    {
        Vector2 input = controls.Dialogue.Move.ReadValue<Vector2>();
        if (input.magnitude < .5f)
        {
            ResetHold();
            return;
        }

        ChoiceDirection dir = Mathf.Abs(input.x) > Mathf.Abs(input.y)
        ? (input.x > 0 ? ChoiceDirection.Right : ChoiceDirection.Left)
        : (input.y > 0 ? ChoiceDirection.Up : ChoiceDirection.Down);

        if (!directionalChoices.ContainsKey(dir))
        {
            ResetHold();
            return;
        }

        if (currentDir != dir)
        {
            ResetHold();
            currentDir = dir;
        }

        HighlightChoice(dir);

        holdTimer += Time.deltaTime;
        UpdateHoldUI(dir, holdTimer / holdTimeToSelect);

        if (holdTimer >= holdTimeToSelect)
        {
            waitingForHoldCompletion = false;
            SelectChoice(directionalChoices[dir]);
            ResetHold();
        }
    }

    // Highlight choices when player presses or holds in direction of answer choices
    void HighlightChoice(ChoiceDirection dir)
    {
        foreach (var obj in spawnedChoices)
        {
            obj.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }

        NewDialogueChoiceData choice = directionalChoices[dir];

        GameObject target = spawnedChoices.Find(o =>
            o.GetComponentInChildren<TextMeshProUGUI>().text == choice.text);

        if (!target) return;

        TextMeshProUGUI txt = target.GetComponentInChildren<TextMeshProUGUI>();

        // change color of text based on morality change value
        txt.color =
            choice.moralityChange > 0 ? Color.green :
            choice.moralityChange < 0 ? Color.red :
            Color.yellow;
    }

    // update the hold UI to show how long the player needs to hold and give feedback to player
    void UpdateHoldUI(ChoiceDirection dir, float progress)
    {
        if (!holdMap.ContainsKey(dir)) return;

        var visual = holdMap[dir];

        visual.image.gameObject.SetActive(true);
        visual.image.fillAmount = Mathf.Clamp01(progress);
    }

    // reset hold feedback if player lets go of directional input
    void ResetHold()
    {
        holdTimer = 0;
        currentDir = null;

        foreach (var v in holdMap.Values)
        {
            v.image.fillAmount = 0;
            v.image.gameObject.SetActive(false);
        }

        // player released after timer expired
        if (waitingForHoldCompletion && canChoose)
        {
            waitingForHoldCompletion = false;
            SelectChoice(GetBiasedChoice(new List<NewDialogueChoiceData>(directionalChoices.Values)));
        }
    }

    // select the choice
    private void SelectChoice(NewDialogueChoiceData c)
    {
        if (activeDialogueTrigger != null)
            activeDialogueTrigger.StopPlayerThinking(); //Call NPC to stop the Player thinking animation when a decision is made

        resolvingChoice = true;
        canChoose = false;

        ClearChoices();
        StartCoroutine(ResolveChoiceRoutine(c));
    }

    // use the correct audio mixer to match character gender
    void SetVoiceGender(string gender)
    {
        if (string.IsNullOrEmpty(gender)) return;

        typingSource.outputAudioMixerGroup = gender.ToLower() == "male" ? maleVoiceGroup : femaleVoiceGroup;
    }

    // set the correct NPC portrait intended for the current line
    void SetNPCPortrait(LineTone tone)
    {
        NPCPortraitSet set = npcPortraitSets.Find(p => p.npcName == currentLine.Speaker);
        if (set == null) return;

        npcPortrait.sprite = tone switch
        {
            LineTone.Happy => set.happy,
            LineTone.Upset => set.upset,
            _ => set.neutral
        };
        
        npcPortrait.SetNativeSize();
        npcPortrait.gameObject.SetActive(true);
    }

    void PlayTypingSound(char c)
    {
        if (char.IsWhiteSpace(c)) return;

        char up = char.ToUpper(c);
        if (letterSounds.ContainsKey(up))
        {
            typingSource.PlayOneShot(letterSounds[up], 0.7f);
        }
    }

    // get the directional position of the answer choices
    Vector2 GetDirPos(ChoiceDirection dir)
    {
        return dir switch
        {
            ChoiceDirection.Up => new Vector2(0, choiceDistance),
            ChoiceDirection.Down => new Vector2(0, -choiceDistance),
            ChoiceDirection.Left => new Vector2(-choiceDistance * 2.2f, 0),
            ChoiceDirection.Right => new Vector2(choiceDistance * 2.2f, 0),
            _ => Vector2.zero
        };
    }
    
    // clear answer choices
    void ClearChoices()
    {
        foreach (var c in spawnedChoices)
        {
            Destroy(c);
        }

        spawnedChoices.Clear();
    }

    // show pop up message of morality change and current total morality
    void ShowPopup(string msg)
    {
        popupText.text = msg;
        popupText.alpha = 1f;
        popupText.gameObject.SetActive(true);
        StartCoroutine(FadePopup());
    }

    // make pop up fade away after selection
    IEnumerator FadePopup()
    {
        yield return new WaitForSeconds(1f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            popupText.alpha = 1 - t;
            yield return null;
        }

        popupText.gameObject.SetActive(false);
    }

    // end the current dialogue instance
    public void EndDialogue()
    {
        if (activeDialogueTrigger != null)
            activeDialogueTrigger.StopLookingAtPlayer(); //Call NPC to stop looking at player to end dialogue animations

        DialogueIsActive = false;

        // deactivate dialogue UI
        continueArrow.SetActive(false);
        dialoguePanel.SetActive(false);
        playerPortrait.gameObject.SetActive(false);

        if (playerController != null)
        {
            playerController.SetDialogueActive(false);
        }

        if (activeDialogueTrigger != null)
        {
            activeDialogueTrigger.OnDialogueComplete();
            if (currentLine.GiveItem)
            {
                activeDialogueTrigger.GiveReward();
            }
        }

        // return camera to original position
        StartCoroutine(cam.EndCameraZoom());
        cam.SetCameraLocked(false);
        npcPortrait.gameObject.SetActive(false);
    }

}
