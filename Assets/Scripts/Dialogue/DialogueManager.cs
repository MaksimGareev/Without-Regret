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

    // Player references
    private Transform playerTransform;
    private PlayerThrowing playerThrowing;
    private PlayerFloating playerFloating;
    private PlayerController playerController;

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
            MoveInput = 0f;
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
        //PopupText.gameObject.SetActive(false);
        dialogueVariables["Morality"] = 0;

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
        playerTransform = player.transform;
        playerFloating = player.GetComponent<PlayerFloating>();
        playerThrowing = player.GetComponent<PlayerThrowing>();
        playerController = player.GetComponent<PlayerController>();
        
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
        StopAllCoroutines();
        StartCoroutine(TypeLine(line.text));

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

        foreach (char c in text)
        {
            DialogueText.text += c;
            char upperChar = char.ToUpper(c);
            if (letterSounds.ContainsKey(upperChar))
            {
                TypingAudioSource.PlayOneShot(letterSounds[upperChar]);
            }
            yield return new WaitForSeconds(0.02f);
        }

        IsTyping = false;

        // Show choices if any
        var choices = currentDialogue.dialogueLines[currentIndex].choices;
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
        CanChoose = false;
        Debug.Log($"OnChoiceSelected CALLED. Choice text: {chosen.text}");

        // Get the current lines choices
        // var choices = currentDialogue.dialogueLines[currentIndex].choices;
        //DialogueChoice chosen = null;

        // Find which choice triggered this next index
        /* foreach (var choice in choices)
         {
             if (choice.nextIndex == nextIndex)
             {
                 chosen = choice;
                 break;
             }
         }*/

        // Apply variable change if any
        if (!string.IsNullOrEmpty(chosen.morality))
        {
            if (!dialogueVariables.ContainsKey(chosen.morality))
            {
                dialogueVariables[chosen.morality] = 0;
            }

            dialogueVariables[chosen.morality] += chosen.valueChange;

            Debug.Log($"{chosen.morality} changed by {chosen.valueChange}. New value: {dialogueVariables[chosen.morality]}");
        }

        // clear old choices
        foreach (var b in spawnedChoices) Destroy(b);
        spawnedChoices.Clear();
        ChoicesContainer.gameObject.SetActive(false);

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

    }

    public void ShowPopUp(string message, float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(ShowPopupRoutine(message, duration));
    }

    private IEnumerator ShowPopupRoutine(string message, float duration)
    {
        //PopupText.gameObject.SetActive(true);
        PopupText.text = message;
        PopupText.alpha = 1;

        // Fade out over time
        yield return new WaitForSeconds(duration);

        float fadeSpeed = 2f;
        while (PopupText.alpha > 0)
        {
            PopupText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        PopupText.text = "";
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
