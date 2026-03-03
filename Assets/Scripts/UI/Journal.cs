using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Journal : MonoBehaviour
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
    // [SerializeField] private Button charactersTab;

    [Header("Objectives")]
    [SerializeField] private GameObject objectivesPage;
    [SerializeField] private Button[] objectiveButtons;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;
    [SerializeField] private TextMeshProUGUI objectiveProgressText;
    [SerializeField] private Color completedObjectiveColor = Color.gray; 
    [SerializeField] private Color highlightedColor = Color.yellow;
    [SerializeField] private Color originalColor = Color.white;

    //[Header("Characters")]
    //[SerializeField] private GameObject charactersPage;

    //[Header("Canvases")]
    //[SerializeField] private Canvas[] canvasesToDisable;
    [HideInInspector] public bool isJournalOpen = false;

    private List<ObjectiveInstance> objectivesList;
    private int currentObjectiveIndex = 0;

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
        RefreshObjectives();
        OnObjectiveSelect(0);
        journalUI.SetActive(false);

        DisableJournalInput();
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

    private void InitializeButtons()
    {
        for (int i = 0; i < objectiveButtons.Length; i++)
        {
            int index = i;
            objectiveButtons[i].onClick.AddListener(() => OnObjectiveSelect(index));
        }

        objectivesTab.onClick.AddListener(() => OpenObjectivesPage());
        // charactersTab.onClick.AddListener(() => OpenCharactersPage());
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

        // foreach (Canvas canvas in canvasesToDisable)
        // {
        //     if (canvas != null)
        //         canvas.enabled = !isJournalOpen;
        // }
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
        //charactersPage.SetActive(false);
        RefreshObjectives();
        OnObjectiveSelect(0);
    }

    private void OpenCharactersPage()
    {
        //charactersPage.SetActive(true);
        objectivesPage.SetActive(false);
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
        currentObjectiveIndex = index;

        if (objectivesList.Count > index)
        {
            var instance = objectivesList[index];
            objectiveDescriptionText.text = instance.data.description;
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
                    buttonText.text += " (Completed)";
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
}
