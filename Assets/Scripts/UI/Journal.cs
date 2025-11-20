using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{
    [Header("Journal")]
    [SerializeField] private GameObject journalUI;

    [Header("Input")]
    [SerializeField] private KeyCode toggleJournalKey = KeyCode.Tab;
    [SerializeField] private string toggleJournalButton = "Xbox Select Button";

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

    [Header("Canvases")]
    [SerializeField] private Canvas[] canvasesToDisable;

    private List<ObjectiveInstance> objectivesList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeButtons();
        RefreshObjectives();
        OnObjectiveSelect(0);
        journalUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleJournalKey) || Input.GetButtonDown(toggleJournalButton))
        {
            ToggleJournalUI();
        }
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < objectiveButtons.Length; i++)
        {
            int index = i;
            objectiveButtons[i].onClick.AddListener(() => OnObjectiveSelect(index));
        }

        objectivesTab.onClick.AddListener(() => OpenObjectivesPage());
        charactersTab.onClick.AddListener(() => OpenCharactersPage());
    }

    private void ToggleJournalUI()
    {
        journalUI.SetActive(!journalUI.activeSelf);

        if (journalUI.activeSelf)
        {
            RefreshObjectives();
            OnObjectiveSelect(0);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }

        foreach (Canvas canvas in canvasesToDisable)
        {
            canvas.enabled = !journalUI.activeSelf;
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
    }

    public void OnObjectiveSelect(int index)
    {
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

        objectivesList.AddRange(activeObjectives);
        objectivesList.AddRange(completedObjectives);
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
}
