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
    private InputAction tabLeftAction;
    private InputAction tabRightAction;
    [SerializeField] private float navigationCooldown = 0.2f;
    private float navigateTimer = 0f;
    private bool canNavigate = true;

    [Header("Tabs")]
    [SerializeField] private Button objectivesTab;
    [SerializeField] private Button charactersTab;
    [SerializeField] private Button collectiblesTab;
    [SerializeField] private bool rememberLastTab = true;

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

    [Header("Collectibles")]
    [SerializeField] private GameObject collectiblesPage;
    [SerializeField] private Button[] collectiblesButtons;
    [SerializeField] private TextMeshProUGUI collectibleDescriptionText;

    [Header("UI Selection Arrow")]
    [Tooltip("Small arrow RectTransform (UI Image) used to indicate the currently selected tab.")]
    [SerializeField] private RectTransform selectionArrow;
    [Tooltip("Horizontal spacing in pixels between button left edge and the arrow.")]
    [SerializeField] private float arrowSpacing = 8f;

    [HideInInspector] public bool isJournalOpen = false;

    private List<ObjectiveInstance> objectivesList;
    private int currentObjectiveIndex = 0;
    private readonly List<string> characterNamesList = new(); // List to maintain the order of character entries
    private readonly Dictionary<string, string> characterDictionary = new();
    private int currentCharacterIndex = 0;
    private readonly List<string> collectibleNamesList = new();
    private readonly Dictionary<string, string> collectibleDictionary = new();
    private int currentCollectibleIndex = 0;

    private enum JournalPage
    {
        Objectives = 0,
        Characters,
        Collectibles
    }

    private JournalPage currentPage = JournalPage.Objectives;

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

        tabLeftAction = inputActions.FindAction("UI/TabLeft");
        tabLeftAction.Enable();

        tabRightAction = inputActions.FindAction("UI/TabRight");
        tabRightAction.Enable();
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

        if (selectionArrow != null)
        {
            // hide by default
            selectionArrow.gameObject.SetActive(false);
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

    // Save and load information for the character and collectible pages
    public void SaveTo(SaveData data)
    {
        data.journalSaveData.characterEntryList.Clear();
        foreach (var entry in characterDictionary)
        {
            // Add each entry from the dictionary to the save data list
            data.journalSaveData.characterEntryList.Add(new CharacterEntry(entry.Key, entry.Value));
        }

        data.journalSaveData.collectibleEntryList.Clear();
        foreach (var entry in collectibleDictionary)
        {
            data.journalSaveData.collectibleEntryList.Add(new CollectibleEntry(entry.Key, entry.Value));
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

        collectibleDictionary.Clear();
        collectibleNamesList.Clear();
        foreach (var entry in data.journalSaveData.collectibleEntryList)
        {
            AddCollectibleEntry(entry.collectibleName, entry.description);
        }
        RefreshCollectibles();
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

        for (int k = 0; k < collectiblesButtons.Length; k++)
        {
            int index = k; // Capture the current value of k for the lambda
            collectiblesButtons[k].onClick.AddListener(() => OnCollectibleSelect(index));
        }

        objectivesTab.onClick.AddListener(() => OpenObjectivesPage());
        charactersTab.onClick.AddListener(() => OpenCharactersPage());
        collectiblesTab.onClick.AddListener(() => OpenCollectiblesPage());
    }

    private void ToggleJournalUI()
    {
        journalUI.SetActive(!journalUI.activeSelf);
        isJournalOpen = journalUI.activeSelf;

        if (isJournalOpen)
        {
            // Show last or default page
            if (!rememberLastTab) currentPage = JournalPage.Objectives;
            
            switch (currentPage)
            {
                case JournalPage.Characters:
                    OpenCharactersPage();
                    break;
                case JournalPage.Collectibles:
                    OpenCollectiblesPage();
                    break;
                default:
                    OpenObjectivesPage();
                    break;
            }

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

            // hide arrow when closing
            if (selectionArrow != null)
            {
                selectionArrow.gameObject.SetActive(false);
            }
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
        if (objectivesPage == null || charactersPage == null || collectiblesPage == null)
        {
            Debug.LogError("One or more journal pages are not assigned in the inspector.");
            return;
        }

        MoveArrowToButton(objectivesTab);
        currentPage = JournalPage.Objectives;

        objectivesPage.SetActive(true);
        charactersPage.SetActive(false);
        collectiblesPage.SetActive(false);
        RefreshObjectives();
        currentObjectiveIndex = 0;
        OnObjectiveSelect(0);
    }

    private void OpenCharactersPage()
    {
        if (objectivesPage == null || charactersPage == null || collectiblesPage == null)
        {
            Debug.LogError("One or more journal pages are not assigned in the inspector.");
            return;
        }

        MoveArrowToButton(charactersTab);
        currentPage = JournalPage.Characters;

        charactersPage.SetActive(true);
        objectivesPage.SetActive(false);
        collectiblesPage.SetActive(false);
        RefreshCharacters();
        currentCharacterIndex = 0;
        OnCharacterSelect(0);
    }

    private void OpenCollectiblesPage()
    {
        if (objectivesPage == null || charactersPage == null || collectiblesPage == null)
        {
            Debug.LogError("One or more journal pages are not assigned in the inspector.");
            return;
        }

        MoveArrowToButton(collectiblesTab);
        currentPage = JournalPage.Collectibles;

        charactersPage.SetActive(false);
        objectivesPage.SetActive(false);
        collectiblesPage.SetActive(true);
        RefreshCollectibles();
        currentCollectibleIndex = 0;
        OnCollectibleSelect(0);
    }

    private void HandleControllerNavigation()
    {
        if (!canNavigate) return;

        Vector2 nav = navigateAction.ReadValue<Vector2>();

        // Bumpers: TabLeft/TabRight take priority
        if (tabRightAction != null && tabRightAction.triggered)
        {
            MoveToNextTab();
            canNavigate = false;
            navigateTimer = 0f;
            return;
        }

        if (tabLeftAction != null && tabLeftAction.triggered)
        {
            MoveToPreviousTab();
            canNavigate = false;
            navigateTimer = 0f;
            return;
        }

        // Horizontal axis switches tabs (left/right) as well
        if (nav.x > 0.5f)
        {
            // move right to next tab
            MoveToNextTab();
            canNavigate = false;
            navigateTimer = 0f;
            return;
        }
        else if (nav.x < -0.5f)
        {
            // move left to previous tab
            MoveToPreviousTab();
            canNavigate = false;
            navigateTimer = 0f;
            return;
        }

        // Vertical navigation moves through the current page list
        if (nav.y > 0.5f)
        {
            switch (currentPage)
            {
                case JournalPage.Objectives:
                    if (objectivesList == null || objectivesList.Count == 0) break;
                    int newObjIndexUp = Mathf.Clamp(currentObjectiveIndex - 1, 0, objectivesList.Count - 1);
                    OnObjectiveSelect(newObjIndexUp);
                    break;
                case JournalPage.Characters:
                    if (characterNamesList.Count == 0) break;
                    int newCharIndexUp = Mathf.Clamp(currentCharacterIndex - 1, 0, characterNamesList.Count - 1);
                    OnCharacterSelect(newCharIndexUp);
                    break;
                case JournalPage.Collectibles:
                    if (collectibleNamesList.Count == 0) break;
                    int newColIndexUp = Mathf.Clamp(currentCollectibleIndex - 1, 0, collectibleNamesList.Count - 1);
                    OnCollectibleSelect(newColIndexUp);
                    break;
            }

            canNavigate = false;
            navigateTimer = 0f;
        }
        else if (nav.y < -0.5f)
        {
            switch (currentPage)
            {
                case JournalPage.Objectives:
                    if (objectivesList == null || objectivesList.Count == 0) break;
                    int newObjIndexDown = Mathf.Clamp(currentObjectiveIndex + 1, 0, objectivesList.Count - 1);
                    OnObjectiveSelect(newObjIndexDown);
                    break;
                case JournalPage.Characters:
                    if (characterNamesList.Count == 0) break;
                    int newCharIndexDown = Mathf.Clamp(currentCharacterIndex + 1, 0, characterNamesList.Count - 1);
                    OnCharacterSelect(newCharIndexDown);
                    break;
                case JournalPage.Collectibles:
                    if (collectibleNamesList.Count == 0) break;
                    int newColIndexDown = Mathf.Clamp(currentCollectibleIndex + 1, 0, collectibleNamesList.Count - 1);
                    OnCollectibleSelect(newColIndexDown);
                    break;
            }

            canNavigate = false;
            navigateTimer = 0f;
        }
    }

    private void MoveToNextTab()
    {
        // cycle: Objectives -> Characters -> Collectibles -> Objectives
        currentPage = (JournalPage)(((int)currentPage + 1) % 3);
        switch (currentPage)
        {
            case JournalPage.Objectives:
                OpenObjectivesPage();
                break;
            case JournalPage.Characters:
                OpenCharactersPage();
                break;
            case JournalPage.Collectibles:
                OpenCollectiblesPage();
                break;
        }
    }

    private void MoveToPreviousTab()
    {
        // cycle backwards
        currentPage = (JournalPage)(((int)currentPage + 2) % 3);
        switch (currentPage)
        {
            case JournalPage.Objectives:
                OpenObjectivesPage();
                break;
            case JournalPage.Characters:
                OpenCharactersPage();
                break;
            case JournalPage.Collectibles:
                OpenCollectiblesPage();
                break;
        }
    }

    public void OnObjectiveSelect(int index)
    {
        Debug.Log($"Objective Select called with index : {index}");
        
        if (index < 0) return;
        
        if (index >= objectiveButtons.Length)
        {
            Debug.LogWarning($"Objective index {index} is out of bounds for objective buttons.");
            return;
        }
        
        if (objectivesList == null || index >= objectivesList.Count)
        {
            Debug.LogWarning($"Objective index {index} is out of bounds for objective entries.");
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

        // highlight button text
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
            characterDescriptionText.text = "";
            return;
        }

        if (characterDictionary.Count > index)
        {
            SetCharacterPageInfo(index);
        }
        else
        {
            characterDescriptionText.text = "";
        }

        currentCharacterIndex = index;

        characterButtons[index].GetComponentInChildren<TextMeshProUGUI>().color = highlightedColor;
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (i != index)
            {
                characterButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = originalColor;
            }
        }
    }

    private void OnCollectibleSelect(int index)
    {
        if (index < 0) return;

        if (index >= collectiblesButtons.Length)
        {
            Debug.LogWarning($"Collectible index {index} is out of bounds for collectible buttons.");
            return;
        }

        if (index >= collectibleDictionary.Count)
        {
            Debug.LogWarning($"Collectible index {index} is out of bounds for collectible entries.");
            collectibleDescriptionText.text = "";
            return;
        }

        currentCollectibleIndex = index;

        if (collectibleNamesList.Count > index)
        {
            collectibleDescriptionText.text = collectibleDictionary[collectibleNamesList[index]];
        }
        else
        {
            collectibleDescriptionText.text = "";
        }

        collectiblesButtons[index].GetComponentInChildren<TextMeshProUGUI>().color = highlightedColor;
        for (int i = 0; i < collectiblesButtons.Length; i++)
        {
            if (i != index)
            {
                collectiblesButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = originalColor;
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

    private void RefreshCollectibles()
    {
        for (int i = 0; i < collectiblesButtons.Length; ++i)
        {
            if (collectibleNamesList.Count > i)
            {
                TextMeshProUGUI buttonText = collectiblesButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = collectibleNamesList[i];
                collectiblesButtons[i].interactable = true;

                if (i == currentCollectibleIndex)
                {
                    // Ensure description is up to date for currently selected collectible
                    collectibleDescriptionText.text = collectibleDictionary[collectibleNamesList[i]];
                }
            }
            else
            {
                TextMeshProUGUI buttonText = collectiblesButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = "";
                collectiblesButtons[i].interactable = false;
            }
        }
    }

    public void AddCharacterEntry(string name, string description)
    {
        if (characterDictionary.ContainsKey(name))
        {
            // Character is already in the list, so update the description
            characterDictionary[name] = description;
            Debug.Log($"Updated character entry for {name} in the journal with description \"{description}.\"");
        }
        else
        {
            // Add character entry to the names list and dictionary
            characterNamesList.Add(name);
            characterDictionary.Add(name, description);
            Debug.Log($"Added character entry for {name} in the journal with description \"{description}.\"");
        }

        RefreshCharacters();
    }

    public void AddCollectibleEntry(string name, string description)
    {
        if (collectibleDictionary.ContainsKey(name))
        {
            // Update existing collectible description
            collectibleDictionary[name] = description;
            Debug.Log($"Updated collectible entry for {name} in the journal with description \"{description}.\"");
        }
        else
        {
            collectibleNamesList.Add(name);
            collectibleDictionary.Add(name, description);
            Debug.Log($"Added collectible entry for {name} in the journal with description \"{description}.\"");
        }

        RefreshCollectibles();
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

                if (i == currentCharacterIndex)
                {
                    SetCharacterPageInfo(i);
                }
            }
            else
            {
                TextMeshProUGUI buttonText = characterButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = "";
                characterButtons[i].interactable = false;
            }
        }
    }

    private void SetCharacterPageInfo(int index)
    {
        if (characterDictionary.Count > index)
        {
            characterDescriptionText.text = characterDictionary[characterNamesList[index]];
        }
        if (npcPortrait != null)
        {
            var portrait = characterPortraits.Find(p => p.name == characterNamesList[index]);
            if (portrait != null)
            {
                npcPortrait.sprite = portrait.portrait;
                npcPortrait.SetNativeSize();
                npcPortrait.gameObject.SetActive(true);
                npcPortrait.enabled = true;
            }
            else
            {
                npcPortrait.gameObject.SetActive(false);
                npcPortrait.enabled = false;
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

    // Moves the selection arrow to the left of the provided tab button.
    private void MoveArrowToButton(Button button)
    {
        if (selectionArrow == null || button == null) 
        {
            Debug.LogWarning("Selection arrow or button is null. Cannot move selection arrow.");
            return;
        }

        if (!button.TryGetComponent<RectTransform>(out var buttonRect)) 
        {
            Debug.LogWarning("Button does not have a RectTransform. Cannot move selection arrow.");
            return;
        }

        Debug.Log($"Moving selection arrow to button: {button.name}");



        // Place arrow to the left of the button with arrowSpacing.
        // Use anchoredPosition relative to the parent (assumes siblings use the same anchor system).
        var btnAnchoredPos = buttonRect.anchoredPosition;
        var btnWidth = buttonRect.rect.width;
        var arrowWidth = selectionArrow.rect.width;

        float arrowX = btnAnchoredPos.x - (btnWidth * 0.5f) - (arrowWidth * (1f - selectionArrow.pivot.x)) - arrowSpacing;
        float arrowY = btnAnchoredPos.y;

        selectionArrow.anchoredPosition = new Vector2(arrowX, arrowY);
        selectionArrow.gameObject.SetActive(true);
    }
}
