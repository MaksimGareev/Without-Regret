using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

[System.Serializable]
public class NPCPortraitSet
{
    public string npcName;
    public Sprite happy;
    public Sprite neutral;
    public Sprite upset;
}

public class DialogueManager : MonoBehaviour
{
    public static bool DialogueIsActive;

    [Header("UI")]
    [Tooltip("Main dialogue panel shown during dialogue interaction")]
    [SerializeField] GameObject DialoguePanel;
    [Tooltip("Name display of the speaker")]
    [SerializeField] TextMeshProUGUI npcNameText;
    [Tooltip("The display text of the current dialogue line")]
    [SerializeField] TextMeshProUGUI dialogueText;
    [Tooltip("The transform containing the dialogue choices")]
    [SerializeField] Transform choicesContainer;
    [Tooltip("The arrow image showing the player which direction to press to select an answer")]
    [SerializeField] GameObject DirectionalImage;
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
    [SerializeField] Image playerPortrait;
    [Tooltip("Sprites that appear in response to different answer choices")]
    [SerializeField] Sprite defaultPortrait, positivePortrait, negativePortrait, neutralPortrait;
    [Tooltip("How long the transition takes between portraits")]
    [SerializeField] float portraitFadeTime = .25f;
    [Tooltip("How long the new portrait stays before reverting back to the default")]
    [SerializeField] float portraitHoldTime = .75f;

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

    [Header("Choice Selection")]
    [Tooltip("How long the player needs to hold to confirm a selection")]
    [SerializeField] float holdTimeToSelect = 1.2f;
    [Tooltip("How much time the player has to make a choice")]
    [SerializeField] float choiceTimeLimit = 15f;
    [Tooltip("How far the answer choices are spaced out from the center of the choice container")]
    [SerializeField] float choiceDistance = 250f;
    Dictionary<ChoiceDirection, HoldDirectionVisual> holdMap = new();

    [Header("NPC movement (trying to remove")]
    private DialogueTrigger activeDialogueTrigger;
    private Irene ireneNPC;
    private Barry barryNPC;
    private DarryNeighborhood darryNPC;
    public Transform ireneDestinationTransform;
    public GameObject IntruderTrigger;

    DialogueData dialogue;

    Dictionary<string, DialogueLine> LineLookup = new();
    DialogueLine currentLine;
    string currentLineID;

    bool typing;
    bool CanChoose;
    bool waitingForHoldCompletion = false;

    Dictionary<char, AudioClip> letterSounds = new();
    Dictionary<ChoiceDirection, DialogueChoice> directionalChoices = new();

    List<GameObject> spawnedChoices = new();

    float holdTimer = 0f;
    ChoiceDirection? currentDir = null;

    bool confirmPressed;

    Coroutine typingRoutine;
    Coroutine timerRoutine;
    Coroutine portraitRoutine;

    CanvasGroup portraitGroup;
    CanvasGroup npcPortraitGroup;

    public int playerMorality;
    int posCount, negCount, neutralCount;

    PlayerController playerController;
    CameraMovement cam;

    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        
        portraitGroup = playerPortrait.GetComponent<CanvasGroup>();
        if (!portraitGroup)
        {
            portraitGroup = playerPortrait.gameObject.AddComponent<CanvasGroup>();
        }

        npcPortraitGroup = npcPortrait.GetComponent<CanvasGroup>();
        if (!npcPortraitGroup)
        {
            npcPortraitGroup = npcPortrait.gameObject.AddComponent<CanvasGroup>();
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
        controls.Dialogue.Move.performed += _ => { };
        controls.Dialogue.Confirm.performed += _ => OnConfirmPressed();
    }

    void BuildLetterSounds()
    {
        for (int i = 0; i < letterClips.Count; i++)
        {
            letterSounds[(char)('A' + i)] = letterClips[i];
        }
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    // Load Json file on start of dialogue interaction
    public void StartDialogueFromJson(TextAsset json, DialogueTrigger trigger)
    {
        dialogue = JsonUtility.FromJson<DialogueData>(json.text);
        LineLookup.Clear();

        foreach (var line in dialogue.dialogueLines)
        {
            LineLookup[line.LineID] = line;

            if (line.choices != null)
            {
                foreach (var choice in line.choices)
                {
                    choice.ParseDirection();
                }
            }
        }

        currentLineID = dialogue.dialogueLines[0].LineID;

        playerController = FindAnyObjectByType<PlayerController>();
        cam = Camera.main.GetComponent<CameraMovement>();

        // assign the current active dialogue trigger
        activeDialogueTrigger = trigger;

        // NPC references for movement if needed
        ireneNPC = FindAnyObjectByType<Irene>();
        barryNPC = FindAnyObjectByType<Barry>();
        darryNPC = FindAnyObjectByType<DarryNeighborhood>();

        // activate dialogue UI
        DialoguePanel.SetActive(true);
        playerPortrait.gameObject.SetActive(true);
        npcNameText.text = dialogue.npcName;

        // set dialogue to true and lock camera rotation
        DialogueIsActive = true;
        playerController.SetDialogueActive(true);
        cam.SetCameraLocked(true);

        if (trigger != null && trigger.focusCameraOnTrigger)
        {
            cam.TriggerDialogueCamera(trigger.transform);
        }

        ShowLine();
    }

    // End dialogue interaction
    public void EndDialogue()
    {
        StopAllCoroutines();

        // deactivate Dialogue UI
        DialoguePanel.SetActive(false);
        playerPortrait.gameObject.SetActive(false);
        continueArrow.SetActive(false);
        ClearChoices();

        // set dialogue interaction to false
        DialogueIsActive = false;
        playerController.SetDialogueActive(false);

        if (activeDialogueTrigger != null)
        {
            // if the dialogue trigger has a reward item, add it to the inventory
            if (activeDialogueTrigger.RewardItem != null)
            {
                Inventory inventory = FindObjectOfType<Inventory>();
                if (inventory != null)
                {
                    inventory.AddItem(activeDialogueTrigger.RewardItem);
                }
                else
                {
                    Debug.LogError("No inventory found in scene to add dialogue reward item to.");
                }
            }
            
            // if the dialogue trigger has a linked objective, add progress to it
            if (activeDialogueTrigger.linkedObjective != null && ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.AddProgress(activeDialogueTrigger.linkedObjective.objectiveID, 1);
            }
        }

        StartCoroutine(cam.EndCameraZoom());
        cam.SetCameraLocked(false);
        npcPortrait.gameObject.SetActive(false);
    }

    // Show current line of dialogue
    void ShowLine()
    {
        // search for Line ID of current line
        if (!LineLookup.ContainsKey(currentLineID))
        {
            EndDialogue();
            return;
        }

        currentLine = LineLookup[currentLineID];

        DirectionalImage.SetActive(false);
        

        npcNameText.text = currentLine.Speaker;

        SetNPCPortrait(currentLine.lineTone);

        SetVoiceGender(currentLine.NPCGender);

        continueArrow.SetActive(false);
        ClearChoices();

        typingRoutine = StartCoroutine(TypeLine(currentLine));

    }

    // type out current line of dialogue and pauses for punctuation
    IEnumerator TypeLine(DialogueLine line)
    {
        typing = true;
        dialogueText.text = "";

        foreach (char c in line.text)
        {
            dialogueText.text += c;
            PlayTypingSound(c);

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

        // activate continue arrow when line is built and there are no choices
        if (line.choices == null || line.choices.Count == 0)
        {
            continueArrow.SetActive(true);
        }
        else
        {
            SpawnChoices(line.choices);
        }
    }

    void SetVoiceGender(string gender)
    {
        if (string.IsNullOrEmpty(gender)) return;

        switch (gender.ToLower())
        {
            case "male":
                if (maleVoiceGroup != null) typingSource.outputAudioMixerGroup = maleVoiceGroup;
                break;
            case "female":
                if (femaleVoiceGroup != null) typingSource.outputAudioMixerGroup = femaleVoiceGroup;
                break;
        }
    }

    void SetNPCPortrait(LineTone lineTone)
    {
        if (npcPortrait == null || currentLine == null) return;

        NPCPortraitSet set = npcPortraitSets.Find(p => p.npcName == currentLine.Speaker);
        if (set == null) return;

        Sprite newSprite = lineTone switch
        {
            LineTone.Happy => set.happy,
            LineTone.Neutral => set.neutral,
            LineTone.Upset => set.upset,
            _ => set.neutral
        };

        npcPortrait.sprite = newSprite;
        npcPortrait.gameObject.SetActive(true);

    }

    void OnConfirmPressed()
    {
        // if dialogue is still being built and confirm is press build out full line
        if (typing)
        {
            SkipTyping();
            return;
        }

        // ignore confirm if chioces are present
        if (CanChoose) return;

        // if current line has endDialogueAfterLine true end dialogue and move NPC if needed
        if (currentLine.endDialogueAfterLine)
        {
            EndDialogue();
            HandleNPCMovementsAfterLine();
            return;
        }

        // if next line does not have NextLineID end dialogue
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

        string npc = activeDialogueTrigger.NPCName;

        // irene follow
        if (npc == "Irene" && ireneNPC != null)
        {
            if (!ireneNPC.IsFollowing)
                ireneNPC.IsFollowing = true;
        }

        // Irene story travel
        if (npc == "IreneStory" && ireneNPC != null)
        {
            ireneNPC.StartTravel();
            ireneNPC.dialogueTrigger.TalkedAlready = true;
        }

        // Irene move after talked
        if (npc == "Irene" && ireneNPC != null && activeDialogueTrigger.TalkedAlready && ireneNPC.GoBackHomeSpot != null)
        {
            ireneNPC.targetSpot = ireneNPC.GoBackHomeSpot;
            ireneNPC.StartTravel();
        }

        // Reed movement
        if (barryNPC != null && (npc == "Reed" || npc == "Darry") && activeDialogueTrigger.TalkedAlready)
        {
            barryNPC.StartTravel();
        }
        else
        {
            Debug.LogWarning("Barry will not move");
        }

        // Darry movement
        if (darryNPC != null && (npc == "Reed" || npc == "Darry"))
        {
            darryNPC.StartTravel();
        }
        else if (darryNPC == null)
        {
            Debug.LogWarning("Darry npc not found in scene");
        }
    }

    // if line is being built and player presses the confirm button build the line immediately
    void SkipTyping()
    {
       if (!typing) return;
          StopCoroutine(typingRoutine);
          dialogueText.text = currentLine.text;
          typing = false;
        

       // After skiping show arrow or choices immediately
       if (currentLine.choices == null || currentLine.choices.Count == 0)
       {
            continueArrow.SetActive(true);
       }
       else
       {
            SpawnChoices(currentLine.choices);
       }
    }

    // spawn answer choices if current line prompts them to
    void SpawnChoices(List<DialogueChoice> choices)
    {
        CanChoose = true;
        directionalChoices.Clear();
        choiceTimerSlider.gameObject.SetActive(true);

        if (DirectionalImage != null)
        {
            DirectionalImage.SetActive(true);
            RectTransform rt = DirectionalImage.GetComponent<RectTransform>();
            rt.SetParent(choicesContainer, false);
            rt.anchoredPosition = Vector2.zero;
        }

       foreach (var c in choices)
       {
            GameObject obj = Instantiate(choicePrefab, choicesContainer);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = c.text;
            obj.GetComponent<RectTransform>().anchoredPosition = GetDirPos(c.direction);

            spawnedChoices.Add(obj);
            directionalChoices[c.direction] = c;
       }

       timerRoutine = StartCoroutine(ChoiceTimer(choices));
    }

    private void Update()
    {
        if (!CanChoose) return;
        HandleDirectionalSelection();
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

        DialogueChoice choice = directionalChoices[dir];

        GameObject target = spawnedChoices.Find(o =>
            o.GetComponentInChildren<TextMeshProUGUI>().text == choice.text);

        if (!target) return;

        TextMeshProUGUI txt = target.GetComponentInChildren<TextMeshProUGUI>();

        txt.color =
            choice.moralityChange > 0 ? Color.green :
            choice.moralityChange < 0 ? Color.red :
            Color.yellow;
    }

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
        if (waitingForHoldCompletion && CanChoose)
        {
            waitingForHoldCompletion = false;
            SelectChoice(GetBiasedChoice(new List<DialogueChoice>(directionalChoices.Values)));
        }
    }

    void SelectChoice(DialogueChoice c)
    {
        CanChoose = false;
        choiceTimerSlider.gameObject.SetActive(false);
        //DirectionalImage.SetActive(false);
        StopCoroutine(timerRoutine);

        ClearChoices();
        StartCoroutine(ResolveChoiceRoutine(c));

    }

    IEnumerator ResolveChoiceRoutine(DialogueChoice c)
    {
        ApplyMorality(c.moralityChange);

        ShowPopup($"Morality changed by {c.moralityChange}. New Morality: {playerMorality}");

        yield return new WaitForSeconds(portraitFadeTime * 2 + portraitHoldTime);

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
    IEnumerator ChoiceTimer(List<DialogueChoice> choices)
    {
        float t = choiceTimeLimit;

        while (t > 0 && CanChoose)
        {
            t -= Time.deltaTime;
            choiceTimerSlider.value = t / choiceTimeLimit;
            yield return null;
        }

        if (!CanChoose) yield break;

        if (currentDir != null)
        {
            waitingForHoldCompletion = true;
            yield break;
        }

        SelectChoice(GetBiasedChoice(choices));
    }

    // Handle auto selection if player does not choose in time, take into account past answers with slight randomness
    DialogueChoice GetBiasedChoice(List<DialogueChoice> choices)
    {
        int dominant = Mathf.Max(posCount, negCount, neutralCount);
        int sign = dominant == posCount ? 1 : dominant == negCount ? -1 : 0;

        DialogueChoice best = choices[0];
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
        DirectionalImage.SetActive(false);
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

    // play sounds of letters when being built
    void PlayTypingSound(char c)
    {
        if (char.IsWhiteSpace(c)) return;

        char up = char.ToUpper(c);
        if (letterSounds.ContainsKey(up))
        {
            typingSource.PlayOneShot(letterSounds[up], .7f);
        }
    }

    // set directional position of answer choices when spawned
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

    // remove answer choices
    void ClearChoices()
    {
        foreach (var c in spawnedChoices)
            c.SetActive(false);
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
} 

