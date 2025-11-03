using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Ink.Runtime;
using Ink.UnityIntegration;

public class DialogueManager : MonoBehaviour
{
    // UI
    public GameObject DialoguePanel;
    public TextMeshProUGUI NPCNameText;
    public TextMeshProUGUI DialogueText;
    public Transform ChoicesContainer;
    public GameObject ChoiceButton;

    private List<GameObject> spawnedChoices = new List<GameObject>();
    private Story currentStory;
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

    private string NPCName;

    private DialogueVariables dialogueVariables;
    [SerializeField] private InkFile globalsInkFile;

    private void Awake()
    {
        controls = new PlayerControls();
        dialogueVariables = new DialogueVariables(globalsInkFile.filePath);

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

    // -------------------- INK LOADING --------------------
    public void StartDialogueFromInk(string NPCName, TextAsset inkJSON)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.transform;
        playerFloating = player.GetComponent<PlayerFloating>();
        playerThrowing = player.GetComponent<PlayerThrowing>();

        this.NPCName = NPCName;
        DialoguePanel.SetActive(true);
        PlayerController.DialogueActive = true;

        if (playerFloating != null) playerFloating.enabled = false;
        if (playerThrowing != null) playerThrowing.enabled = false;

        currentStory = new Story(inkJSON.text);
        dialogueVariables.StartListening(currentStory);

        ContinueStory();
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            string text = currentStory.Continue().Trim();
            NPCNameText.text = NPCName;
            StopAllCoroutines();
            StartCoroutine(TypeLine(text));
        }
        else if (currentStory.currentChoices.Count > 0)
        {
            SpawnChoices();
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine(string line)
    {
        IsTyping = true;
        DialogueText.text = "";

        foreach (char c in line.ToCharArray())
        {
            DialogueText.text += c;
            char upperChar = char.ToUpper(c);
            if (letterSounds.ContainsKey(upperChar))
            {
                TypingAudioSource.PlayOneShot(letterSounds[upperChar]);
            }
            yield return new WaitForSeconds(0.03f);
        }

        IsTyping = false;

        // After typing line, show choices (if any)
        if (currentStory.currentChoices.Count > 0)
        {
            SpawnChoices();
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
        ContinueStory();
    }

    private void SpawnChoices()
    {
        // Clear old
        foreach (var b in spawnedChoices) Destroy(b);
        spawnedChoices.Clear();

        ChoicesContainer.gameObject.SetActive(true);

        for (int i = 0; i < currentStory.currentChoices.Count; i++)
        {
            int index = i;
            var choice = currentStory.currentChoices[i];
            GameObject buttonObj = Instantiate(ChoiceButton, ChoicesContainer);
            buttonObj.SetActive(true);
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.text.Trim();

            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnChoiceSelected(index));
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
            OnChoiceSelected(SelectedChoiceIndex);
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

    private void OnChoiceSelected(int choiceIndex)
    {
        CanChoose = false;
        currentStory.ChooseChoiceIndex(choiceIndex);
        foreach (var b in spawnedChoices) Destroy(b);
        spawnedChoices.Clear();
        ChoicesContainer.gameObject.SetActive(false);
        ContinueStory();
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
        PlayerController.DialogueActive = false;
        dialogueVariables.StopListening(currentStory);

        if (playerFloating != null) playerFloating.enabled = true;
        if (playerThrowing != null) playerThrowing.enabled = true;

        foreach (var b in spawnedChoices) Destroy(b);
        spawnedChoices.Clear();

        if (TypingAudioSource != null) TypingAudioSource.Stop();
    }
}
