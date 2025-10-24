using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Ink.Runtime;
using UnityEngine.EventSystems;
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    private bool dialogueIsPlayeing;

    private Story currentStory;

    private static DialogueManager Instance;
    private PlayerControls controls;

    private void Awake()
    {

        controls = new PlayerControls();
        controls.Dialogue.Continue.performed += ctx => ContinueStory();
        if (Instance != null)
        {
            Debug.LogWarning("Found more than one in scene");
        }
        Instance = this;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
    public static DialogueManager GetInstance()
    {
        return Instance;
    }

    private void Start()
    {
        dialogueIsPlayeing = false;
        dialoguePanel.SetActive(false);

        // get all choices
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlayeing)
        {
            return;
        }

        if (InputManager.GetInstance().GetSubmitPressed())
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlayeing = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
        SelectFirstChoice();

    }

    private void ExitDialogueMode()
    {
        dialogueIsPlayeing = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";

    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices();
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        int index = 0;

        foreach(Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    /*  
      // UI
      public GameObject DialoguePanel;
      public TextMeshProUGUI NPCNameText;
      public TextMeshProUGUI DialogueText;
      public Transform ChoicesContainer;
      public GameObject ChoiceButton;
      private List<GameObject> spawnedChoices = new List<GameObject>();
      private DialogueNode currentNode;
      private string NPCName;

      private Transform playerTransform;
      private bool IsTyping;

      // letter sounds
      public AudioSource TypingAudioSource;
      public List<AudioClip> letterClips;
      private Dictionary<char, AudioClip> letterSounds;

      // Navigation sounds
      public AudioSource uiAudioSource;
      public AudioClip moveClip;
      public AudioClip confirmClip;

      // Selection
      private int SelectiedChoiceIndex = 0;
      private bool CanChoose = false;

      private PlayerThrowing playerThrowing;
      private PlayerFloating playerFloating;

      private PlayerControls controls;
      private float MoveInput;
      private bool MoveUpPressed;
      private bool MoveDownPressed;
      private bool ConfirmPressed;

    private void Awake()
    {
        controls = new PlayerControls();

        // Move input (up/down navigation)
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

        // Confirm (select choice)
        controls.Dialogue.Confirm.performed += ctx => ConfirmPressed = true;
        controls.Dialogue.Confirm.canceled += ctx => ConfirmPressed = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
    void Start()
      {
          DialoguePanel.SetActive(false);

          // Build letter to clip dictionary
          letterSounds = new Dictionary<char, AudioClip>();

          // Make sure the list has 26 clips
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

      public void HandleChoiceInput()
      {
          if (!CanChoose || spawnedChoices.Count == 0) return;

          if (MoveUpPressed)
          {
              SelectiedChoiceIndex = (SelectiedChoiceIndex - 1 + spawnedChoices.Count) % spawnedChoices.Count;
              UpdateChoiceHighlight();
            //uiAudioSource?.PlayOneShot(moveClip);
            MoveUpPressed = false;
          }

          if (MoveDownPressed)
          {
              SelectiedChoiceIndex = (SelectiedChoiceIndex + 1) % spawnedChoices.Count;
              UpdateChoiceHighlight();
            //uiAudioSource?.PlayOneShot(moveClip);
            MoveDownPressed = false;
          }

          if (ConfirmPressed)
          {
              //uiAudioSource?.PlayOneShot(confirmClip);
              OnChoiceSelected(SelectiedChoiceIndex);
            ConfirmPressed = false;
          }
      }

      public void StartDialogue(string NPCName, DialogueNode StartNode)
      {
          GameObject player = GameObject.FindGameObjectWithTag("Player");

          playerTransform = player.transform;
          playerFloating = player.GetComponent<PlayerFloating>();
          playerThrowing = player.GetComponent<PlayerThrowing>();

          this.NPCName = NPCName;
          DialoguePanel.SetActive(true);
          PlayerController.DialogueActive = true;

          if (playerFloating != null)
          {
              playerFloating.enabled = false;
          }

          if (playerThrowing != null)
          {
              playerThrowing.enabled = false;
          }

          ShowNode(StartNode);
      }

      private void ShowNode(DialogueNode node)
      {
          currentNode = node;
          NPCNameText.text = NPCName;

          StopAllCoroutines();
          StartCoroutine(TypeLine(node.npcLine));

          // Clear old buttons
          foreach (var b in spawnedChoices)
              Destroy(b);
          spawnedChoices.Clear();

          // Disable choice selection when the NPC is talking
          CanChoose = false;
          SelectiedChoiceIndex = 0;

          // Hide Choices
          ChoicesContainer.gameObject.SetActive(false);

          // Debug check
          Debug.Log("Spawning " + node.playerChoices.Count + " choice buttons");

      }

      private void SpawnChoices()
      {
          if (currentNode == null || currentNode.playerChoices == null) return;

          // enable container
          ChoicesContainer.gameObject.SetActive(true);

          // Spawn choices
          for (int i = 0; i < currentNode.playerChoices.Count; i++)
          {
              int index = i;
              GameObject buttonObj = Instantiate(ChoiceButton, ChoicesContainer);
              buttonObj.SetActive(true); // ensure it's active
              TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

              if (buttonText != null)
              {
                  buttonText.text = currentNode.playerChoices[i];
              }
              Button btn = buttonObj.GetComponent<Button>();
              if (btn != null)
              {
                  btn.onClick.AddListener(() => OnChoiceSelected(index));
              }
              spawnedChoices.Add(buttonObj);
          }

          // allow player inputs
          CanChoose = true;
          SelectiedChoiceIndex = 0;
          UpdateChoiceHighlight();
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

          // spawn choices after npc is done talking
          SpawnChoices();
      }

      private void UpdateChoiceHighlight()
      {
          for (int i = 0; i < spawnedChoices.Count; i++)
          {
              var text = spawnedChoices[i].GetComponentInChildren<TextMeshProUGUI>();
              if (text != null)
              {
                  text.color = (i == SelectiedChoiceIndex) ? Color.yellow : Color.white;
              }
          }
      }

      private void OnChoiceSelected(int choiceIndex)
      {
          DialogueNode nextNode = null;
          if (currentNode.nextNodes != null && choiceIndex < currentNode.nextNodes.Count)
          {
              nextNode = currentNode.nextNodes[choiceIndex];
          }
          if (nextNode != null)
          {
              ShowNode(nextNode);
          }
          else
          {
              EndDialogue();
          }
      }

      public void EndDialogue()
      {
          DialoguePanel.SetActive(false);
          PlayerController.DialogueActive = false;

          if (playerFloating != null)
          {
              playerFloating.enabled = true;
          }

          if (playerThrowing != null)
          {
              playerThrowing.enabled = true;
          }

          foreach (var b in spawnedChoices)
          {
              Destroy(b);
              spawnedChoices.Clear();
          }

          if (TypingAudioSource != null)
          {
              TypingAudioSource.Stop();
          }

      }*/

}
