using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Journal : MonoBehaviour, ISaveable
{
    [Header("Singleton")]
    public static Journal Instance { get; private set; }

    [Header("Journal")]
    [SerializeField] private GameObject journalUI;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction playerJournalAction;
    private InputAction UIJournalAction;
    private InputAction navigateAction;
    private InputAction cancelAction;
    [SerializeField] private float navigationCooldown = 0.2f;
    private float navigateTimer = 0f;
    private bool canNavigate = true;

    [Header("Tabs")]
    [SerializeField] private Button objectivesTab;
    [SerializeField] private Button charactersTab;

    [Header("Objectives")]
    [SerializeField] private GameObject objectivesPage;
    [SerializeField] private Button[] objectiveButtons;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;
    [SerializeField] private TextMeshProUGUI objectiveProgressText;
    [SerializeField] private Color completedObjectiveColor = Color.gray; 
    [SerializeField] private Color highlightedColor = Color.yellow;
    [SerializeField] private Color originalColor = Color.white;

    [Header("Characters")]
    [SerializeField] private GameObject charactersPage;
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private TextMeshProUGUI characterDescriptionText;
    [SerializeField] private Image npcPortrait;
    [SerializeField] private List<CharacterPortrait> characterPortraits;
    [SerializeField] private JournalEntry echoJournalEntry;
    
    [HideInInspector] public bool isJournalOpen = false;

    private List<ObjectiveInstance> objectivesList;
    private int currentObjectiveIndex = 0;
    private readonly List<string> characterNamesList = new(); // List to maintain the order of character entries
    private readonly Dictionary<string, string> characterDictionary = new();

    private void Awake()
    {
        // Make this a singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize input actions
        playerJournalAction = inputActions.FindAction("Player/Journal");
        playerJournalAction.Enable();

        UIJournalAction = inputActions.FindAction("UI/Journal");
        UIJournalAction.Enable();

        navigateAction = inputActions.FindAction("UI/Navigate");
        navigateAction.Enable();

        cancelAction = inputActions.FindAction("UI/Cancel");
        cancelAction.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeButtons();
        RefreshCharacters();
        OpenObjectivesPage();
        journalUI.SetActive(false);

        DisableJournalInput();

        if (echoJournalEntry)
        {
            AddCharacterEntry(echoJournalEntry.entryTitle, echoJournalEntry.entryDescription);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            return; // Do not allow pausing in the main menu
        }

        if ((playerJournalAction.triggered || UIJournalAction.triggered) && !PauseManager.Instance.isGamePaused && !DialogueManager.DialogueIsActive && !GameOverManager.Instance.IsGameOver)
        {
            ToggleJournalUI();
        }

        if (cancelAction.triggered && isJournalOpen)
        {
            ToggleJournalUI();
        }

        if (isJournalOpen)
        {
            navigateTimer += Time.unscaledDeltaTime;

            if (navigateTimer >= navigationCooldown && !canNavigate)
            {
                canNavigate = true;
            }
        }

        HandleControllerNavigation();
    }

    // Save and load information for the character page
    public void SaveTo(SaveData data)
    {
        data.journalSaveData.characterEntryList.Clear();
        foreach (var entry in characterDictionary)
        {
            // Add each entry from the dictionary to the save data list
            data.journalSaveData.characterEntryList.Add(new CharacterEntry(entry.Key, entry.Value));
        }
    }

    public void LoadFrom(SaveData data)
    {
        characterDictionary.Clear();
        characterNamesList.Clear();
        foreach (var entry in data.journalSaveData.characterEntryList)
        {
            AddCharacterEntry(entry.characterName, entry.description);
        }
        RefreshCharacters();
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < objectiveButtons.Length; i++)
        {
            int index = i; // Capture the current value of i for the lambda
            objectiveButtons[i].onClick.AddListener(() => OnObjectiveSelect(index));
        }

        for (int j = 0; j < characterButtons.Length; j++)
        {
            int index = j; // Capture the current value of j for the lambda
            characterButtons[j].onClick.AddListener(() => OnCharacterSelect(index));
        }

        objectivesTab.onClick.AddListener(() => OpenObjectivesPage());
        charactersTab.onClick.AddListener(() => OpenCharactersPage());
    }

    private void ToggleJournalUI()
    {
        journalUI.SetActive(!journalUI.activeSelf);
        isJournalOpen = journalUI.activeSelf;

        if (isJournalOpen)
        {
            RefreshObjectives();
            OnObjectiveSelect(0);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
            EnableJournalInput();
            DisableOtherCanvases();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
            DisableJournalInput();
            EnableOtherCanvases();
        }
    }

    private void EnableOtherCanvases()
    {
        Debug.Log("Enabling other canvases from Journal");
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.mainCanvas != null && !GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(true);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && !GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(true);
        }

        if (GameManager.Instance.playerUICanvas != null && !GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(true);
        }

        if (GameManager.Instance.gameOverCanvas != null && !GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(GameOverManager.Instance.IsGameOver);
        }

        if (GameManager.Instance.objectivePanel != null && !GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(GameManager.Instance.objectiveCanvas.IsVisible());
        }
    }

    private void DisableOtherCanvases()
    {
        Debug.Log("Disabling other canvases from Journal");
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.mainCanvas != null && GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(false);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(false);
        }

        if (GameManager.Instance.playerUICanvas != null && GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(false);
        }

        if (GameManager.Instance.gameOverCanvas != null && GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(false);
        }

        if (GameManager.Instance.objectivePanel != null && GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(false);
        }
    }

    private void OpenObjectivesPage()
    {
        objectivesPage.SetActive(true);
        charactersPage.SetActive(false);
        RefreshObjectives();
        OnObjectiveSelect(0);
    }

    private void OpenCharactersPage()
    {
        charactersPage.SetActive(true);
        objectivesPage.SetActive(false);
        RefreshCharacters();
        OnCharacterSelect(0);
    }

    private void HandleControllerNavigation()
    {
        if (!canNavigate) return;

        float navigationInput = navigateAction.ReadValue<Vector2>().y;

        if (navigationInput > 0.5f)
        {
            // Move up in the objectives list
            int newIndex = Mathf.Clamp(currentObjectiveIndex - 1, 0, objectivesList.Count - 1);
            OnObjectiveSelect(newIndex);
            canNavigate = false;
            navigateTimer = 0f;
        }
        else if (navigationInput < -0.5f)
        {
            // Move down in the objectives list
            int newIndex = Mathf.Clamp(currentObjectiveIndex + 1, 0, objectivesList.Count - 1);
            OnObjectiveSelect(newIndex);
            canNavigate = false;
            navigateTimer = 0f;
        }
    }

    public void OnObjectiveSelect(int index)
    {
        Debug.Log($"Objective Select called with index : {index}");
        
        if (index < 0) return;
        
        if (index >= objectiveButtons.Length)
        {
            Debug.LogWarning($"Character index {index} is out of bounds for character buttons.");
            return;
        }
        
        if (index >= objectivesList.Count)
        {
            Debug.LogWarning($"Character index {index} is out of bounds for character entries.");
            return;
        }
        
        currentObjectiveIndex = index;

        if (objectivesList.Count > index)
        {
            var instance = objectivesList[index];
            if (!instance.isCompleted)
            {
                objectiveDescriptionText.text = instance.data.description;
            }
            else
            {
                objectiveDescriptionText.text = instance.data.recap;
            }
            
            objectiveProgressText.text = $"Progress: {instance.currentProgress} / {instance.data.requiredProgress}";
        }
        else
        {
            objectiveDescriptionText.text = "";
            objectiveProgressText.text = "";
        }

        objectiveButtons[index].GetComponentInChildren<TextMeshProUGUI>().color = highlightedColor;

        for (int i = 0; i < objectiveButtons.Length; i++)
        {
            if (i != index)
            {
                objectiveButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = originalColor;
            }
        }
    }

    private void OnCharacterSelect(int index)
    {
        Debug.Log($"Character Select called with index : {index}");
        
        if (index < 0) return;
        
        if (index >= characterButtons.Length)
        {
            Debug.LogWarning($"Character index {index} is out of bounds for character buttons.");
            return;
        }
        
        if (index >= characterDictionary.Count)
        {
            Debug.LogWarning($"Character index {index} is out of bounds for character entries.");
            return;
        }
        
        if (characterDictionary.Count > index)
        {
            characterDescriptionText.text = characterDictionary[characterNamesList[index]];

            if (npcPortrait != null)
            {
                var portrait = characterPortraits.Find(p => p.name == characterNamesList[index]);
                if (portrait != null)
                {
                    npcPortrait.sprite = portrait.portrait;
                    npcPortrait.SetNativeSize();
                    npcPortrait.enabled = true;
                }
                else
                {
                    npcPortrait.enabled = false;
                }
            }
        }
        else
        {
            characterDescriptionText.text = "";
        }
        
        characterButtons[index].GetComponentInChildren<TextMeshProUGUI>().color = highlightedColor;
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (i != index)
            {
                characterButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = originalColor;
            }
        }
    }

    private void RefreshObjectiveDatas()
    {
        objectivesList = new List<ObjectiveInstance>();

        var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
        var completedObjectives = ObjectiveManager.Instance.GetCompletedObjectives();

        objectivesList.AddRange(completedObjectives);
        objectivesList.AddRange(activeObjectives);

        // reverse objectivelist
        for (int i = 0; i < objectivesList.Count / 2; i++)
        {
            var temp = objectivesList[i];
            objectivesList[i] = objectivesList[objectivesList.Count - i - 1];
            objectivesList[objectivesList.Count - i - 1] = temp;
        }
    }

    private void RefreshObjectives()
    {
        RefreshObjectiveDatas();

        for (int i = 0; i < objectiveButtons.Length; i++)
        {
            if (objectivesList.Count > i)
            {
                var instance = objectivesList[i];
                var data = instance.data;

                TextMeshProUGUI buttonText = objectiveButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = data.title;

                if (instance.isCompleted)
                {
                    // Add strikethrough to completed objective text
                    buttonText.text = $"<u thickness=15 offset=30>{data.title}</u>";
                    buttonText.color = completedObjectiveColor;
                }
                else
                {
                    buttonText.color = originalColor;
                }

                objectiveButtons[i].interactable = true;
            }
            else
            {
                TextMeshProUGUI buttonText = objectiveButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = "";
                objectiveButtons[i].interactable = false;
            }
        }
    }

    public void AddCharacterEntry(string name, string description)
    {
        if (characterDictionary.ContainsKey(name))
        {
            // Character is already in the list, so update the description
            characterDictionary[name] = description;
        }
        else
        {
            // Add character entry to the names list and dictionary
            characterNamesList.Add(name);
            characterDictionary.Add(name, description);
        }
    }

    private void RefreshCharacters()
    {
        for (int i = 0; i < characterButtons.Length; ++i)
        {
            if (characterNamesList.Count > i)
            {
                TextMeshProUGUI buttonText = characterButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = characterNamesList[i];
                characterButtons[i].interactable = true;
            }
            else
            {
                TextMeshProUGUI buttonText = characterButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = "";
                characterButtons[i].interactable = false;
            }
        }
    }

    private void OnDisable()
    {
        DisableJournalInput();
    }

    private void EnableJournalInput()
    {
        inputActions.FindActionMap("UI").Enable();
        inputActions.FindActionMap("Player").Disable();
    }

    private void DisableJournalInput()
    {
        inputActions.FindActionMap("UI").Disable();
        inputActions.FindActionMap("Player").Enable();
    }

    [Serializable]
    private class CharacterPortrait
    {
        public string name;
        public Sprite portrait;
    }
}
