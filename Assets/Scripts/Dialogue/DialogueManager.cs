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

    private List<GameObject> spawnedChoices = new List<GameObject>();
    private DialogueData currentDialogue;
    private int currentIndex = 0;
    private bool IsTyping;

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

    // Selection
    private int SelectedChoiceIndex = 0;
    private bool CanChoose = false;
    public TextMeshProUGUI PopupText;

    // Typing
    private Coroutine typeingRoutine;
    private string currentFullLine = "";

    // Random choice timer
    public float choiceTimeLimit = 5f;
    private float choiceTimer;
    private Coroutine choiceTimerRoutine;
    public TextMeshProUGUI TimerText;

    // Player references
    private Transform playerTransform;
    private PlayerThrowing playerThrowing;
    private PlayerFloating playerFloating;
    private PlayerController playerController;

    // NPC references
    private Irene ireneNPC;

    // Player morality
    private int playerMorality = 0;

    private string NPCName;
    private Dictionary<string, int> dialogueVariables = new Dictionary<string, int>();

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Dialogue.Move.performed += ctx =>
        {
            MoveInput = ctx.ReadValue<float>();
            if (MoveInput > 0) MoveUpPressed = true;
            if (MoveInput < 0) MoveDownPressed = true;
        };
        controls.Dialogue.Move.canceled += ctx =>
        {
            //MoveInput = 0f;
            MoveUpPressed = false;
            MoveDownPressed = false;
        };

        controls.Dialogue.Confirm.performed += ctx => ConfirmPressed = true;
        controls.Dialogue.Confirm.canceled += ctx => ConfirmPressed = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Start()
    {
        DialoguePanel.SetActive(false);
        playerMorality = 0;
        PlayerPrefs.SetInt("Morality", playerMorality);
        PlayerPrefs.Save();
        //playerMorality = PlayerPrefs.GetInt("Morality", 0);

        // Build letter sound dictionary
        letterSounds = new Dictionary<char, AudioClip>();
        for (int i = 0; i < letterClips.Count && i < 26; i++)
        {
            char letter = (char)('A' + i);
            letterSounds[letter] = letterClips[i];
        }
    }

    void Update()
    {
        HandleChoiceInput();
    }

    // -------------------- JSON Dialogue --------------------
    public void StartDialogueFromJson(TextAsset jsonFile)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        ireneNPC = FindObjectOfType<Irene>();
        playerTransform = player.transform;
        playerFloating = player.GetComponent<PlayerFloating>();
        playerThrowing = player.GetComponent<PlayerThrowing>();
        playerController = player.GetComponent<PlayerController>();
        PopupText.gameObject.SetActive(false);

        if (jsonFile == null)
        {
            Debug.LogWarning("no json dialogue file assigned");
            return;
        }

        currentDialogue = JsonUtility.FromJson<DialogueData>(jsonFile.text);
        currentIndex = 0;

        if (currentDialogue == null || currentDialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning("Dialogue JSON invalid or empty");
            return;
        }

        DialoguePanel.SetActive(true);
        NPCNameText.text = currentDialogue.npcName;
        playerController.SetDialogueActive(true);

        if (playerFloating != null) playerFloating.enabled = false;
        if (playerThrowing != null) playerThrowing.enabled = false;

        ShowCurrentLine();

    }

    private void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.dialogueLines[currentIndex];

        if (line.requiredMorality != 0)
        {
            if ((line.requiredMorality > 0 && playerMorality < line.requiredMorality) || (line.requiredMorality < 0 && playerMorality > line.requiredMorality))
            {
                currentIndex++;
                if (currentIndex < currentDialogue.dialogueLines.Count)
                {
                    ShowCurrentLine();
                }
                else
                {
                    EndDialogue();
                }
                return;
            }
        }

        // Activate objective immediately when line is shown
        if (line.objectivesToActivate != null && line.objectivesToActivate.Count > 0)
        {
            foreach (string objectiveID in line.objectivesToActivate)
            {
                if (!string.IsNullOrEmpty(objectiveID))
                {
                    Debug.Log("Activating objective from dialogue line: " + objectiveID);
                    ObjectiveManager.Instance.ActivateObjectiveByID(objectiveID);
                }
            }
        }
        /*
        StopCoroutine(nameof(TypeLine));
        StartCoroutine(TypeLine(line.text));
        */
        if (typeingRoutine != null)
        {
            StopCoroutine(typeingRoutine);
        }

        typeingRoutine = StartCoroutine(TypeLine(line.text));
        // clear previous choices
        foreach (var b in spawnedChoices)
        {
            Destroy(b);
        }
        spawnedChoices.Clear();
    }

    private IEnumerator TypeLine(string text)
    {
        IsTyping = true;
        DialogueText.text = "";
        currentFullLine = text;

        foreach (char c in text)
        {
            // If skip requested, instantly finish
            if (ConfirmPressed)
            {
                DialogueText.text = currentFullLine;
                ConfirmPressed = false;
                break;
            }

            DialogueText.text += c;
            
            char upperChar = char.ToUpper(c);
            if (letterSounds.ContainsKey(upperChar))
            {
                TypingAudioSource.PlayOneShot(letterSounds[upperChar]);
            }

            yield return new WaitForSeconds(0.02f);
        }

        IsTyping = false;

        // show continue arrow after the line is typed and there are no choices to be selected
        var choices = currentDialogue.dialogueLines[currentIndex].choices;
        if (choices == null || choices.Count == 0)
        {
            ContinueArrow.SetActive(true);
        }
        else if (IsTyping == true)
        {
            ContinueArrow.SetActive(false);
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
    }

    private IEnumerator WaitForNextLine()
    {
        // Wait for player confirm to advance
        while (!ConfirmPressed)
        {
            yield return null;
        }
        ConfirmPressed = false;

        DialogueLine line = currentDialogue.dialogueLines[currentIndex];

        // End dialogue if the line says to
        if (line.endDialogueAfterLine)
        {
            if (ireneNPC != null)
            {
                ireneNPC.StartTravel();
                ireneNPC.IsFollowing = false;
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

    private void SpawnChoices(List<DialogueChoice> choices)
    {
        // Clear old
        foreach (var b in spawnedChoices) Destroy(b);
        spawnedChoices.Clear();
        ChoicesContainer.gameObject.SetActive(true);

        foreach (DialogueChoice choice in choices)
        {
            GameObject buttonObj = Instantiate(ChoiceButton, ChoicesContainer);
            buttonObj.SetActive(true);
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.text;

            Button btn = buttonObj.GetComponent<Button>();
            int next = choice.nextIndex;
            btn.onClick.AddListener(() => OnChoiceSelected(choice));
            spawnedChoices.Add(buttonObj);
        }

        CanChoose = true;
        SelectedChoiceIndex = 0;
        UpdateChoiceHighlight();

        // Start timer countdown for auto-select
        if (choiceTimerRoutine != null)
        {
            StopCoroutine(choiceTimerRoutine);
        }
        choiceTimerRoutine = StartCoroutine(ChoiceTimerCountdown(choices));
    }

    private void HandleChoiceInput()
    {
        if (!CanChoose || spawnedChoices.Count == 0) return;

        if (MoveUpPressed)
        {
            SelectedChoiceIndex = (SelectedChoiceIndex - 1 + spawnedChoices.Count) % spawnedChoices.Count;
            UpdateChoiceHighlight();
            MoveUpPressed = false;
        }

        if (MoveDownPressed)
        {
            SelectedChoiceIndex = (SelectedChoiceIndex + 1) % spawnedChoices.Count;
            UpdateChoiceHighlight();
            MoveDownPressed = false;
        }

        if (ConfirmPressed)
        {
            OnChoiceSelected(currentDialogue.dialogueLines[currentIndex].choices[SelectedChoiceIndex]);
            ConfirmPressed = false;
        }
    }

    private void UpdateChoiceHighlight()
    {
        for (int i = 0; i < spawnedChoices.Count; i++)
        {
            var text = spawnedChoices[i].GetComponentInChildren<TextMeshProUGUI>();
            text.color = (i == SelectedChoiceIndex) ? Color.yellow : Color.white;
        }
    }

    private void OnChoiceSelected(DialogueChoice chosen)
    {
        if (choiceTimerRoutine != null)
        {
            StopCoroutine(choiceTimerRoutine);
            choiceTimerRoutine = null;
        }

        CanChoose = false;

        // Apply variable change if any
        playerMorality += chosen.moralityChange;
        PlayerPrefs.SetInt("Morality", playerMorality);
        PlayerPrefs.Save();

        Debug.Log($"Morality changed by {chosen.moralityChange}. New Morality: {playerMorality}");
 
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

        if (TimerText != null)
        {
            TimerText.text = "";
        }

    }

    private IEnumerator ChoiceTimerCountdown(List<DialogueChoice> choices)
    {
        choiceTimer = choiceTimeLimit;

        while (choiceTimer > 0f && CanChoose)
        {
            if (TimerText != null)
            {
                TimerText.gameObject.SetActive(true);
                TimerText.text = Mathf.CeilToInt(choiceTimer).ToString();
            }
            else
            {
                TimerText.gameObject.SetActive(false);
            }
            choiceTimer -= Time.deltaTime;
            yield return null;
        }

        // Timer has expired and the player hasnt chosen anything
        if (CanChoose)
        {
            DialogueChoice fallback = null;

            // Try to find a neutral or negative morality choice
            foreach (var c in choices)
            {
                if (c.moralityChange <= 0)
                {
                    fallback = c;
                    break;
                }
            }

            // If no suitable one, just pick the first
            if (fallback == null && choices.Count > 0)
            {
                fallback = choices[0];
            }

            if (fallback != null)
            {
                Debug.Log("timer expired auto selecting choice: " + fallback.text);
                OnChoiceSelected(fallback);
            }
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
        DialoguePanel.SetActive(false);
        currentDialogue = null;
        currentIndex = 0;
        spawnedChoices.Clear();
        ConfirmPressed = false;
        CanChoose = false;

        PlayerController.DialogueActive = false;
        playerController.SetDialogueActive(false);
        
        if (playerFloating != null) playerFloating.enabled = true;
        if (playerThrowing != null) playerThrowing.enabled = true;
        if (TypingAudioSource != null) TypingAudioSource.Stop();

        // Only auto follwo if Irene does not have a destination to travel to
        if (ireneNPC != null && ireneNPC.NPCNameMatches(NPCNameText.text))
        {
            if (!ireneNPC.isTraveling)
            {
                ireneNPC.IsFollowing = true;
            }
        }

        Debug.Log($"Dialogue ended. Final morality = {playerMorality}");
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
