using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI[] slotTexts = new TextMeshProUGUI[3];
    [SerializeField] private TextMeshProUGUI[] slotObjectives = new TextMeshProUGUI[3];
    [SerializeField] private TextMeshProUGUI[] slotStatus = new TextMeshProUGUI[3];
    
    [Header("Buttons")]
    [SerializeField] public Button[] playButtons = new Button[3];
    [SerializeField] private Button[] deleteButtons = new Button[3];
    [SerializeField] public Button[] newGameButtons = new Button[3];

    private string firstLevelName = "Echo'sHouse";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetUpEvents();
    }

    private void OnEnable()
    {
        UpdateAllSlots();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateAllSlots()
    {
        for (int i = 1; i <= 3; i++)
        {
            SaveData data = SaveSystem.Load(i);
            UpdateSlotInfo(i, data);
        }

        MainMenu mainMenu = FindAnyObjectByType<MainMenu>();
        mainMenu.SelectSaveMenuButton();
    }

    private void SetUpEvents()
    {
        playButtons[0].onClick.AddListener(() => PlaySelectedSave(1));
        deleteButtons[0].onClick.AddListener(() => ClearSelectedSave(1));
        newGameButtons[0].onClick.AddListener(() => NewGame(1));

        playButtons[1].onClick.AddListener(() => PlaySelectedSave(2));
        deleteButtons[1].onClick.AddListener(() => ClearSelectedSave(2));
        newGameButtons[1].onClick.AddListener(() => NewGame(2));
        
        playButtons[2].onClick.AddListener(() => PlaySelectedSave(3));
        deleteButtons[2].onClick.AddListener(() => ClearSelectedSave(3));
        newGameButtons[2].onClick.AddListener(() => NewGame(3));
    }

    public void UpdateSlotInfo(int slot, SaveData data)
    {
        slotTexts[slot - 1].text = data != null 
        ? $"Save {slot}" 
        : "Empty";

        slotStatus[slot - 1].text = data != null 
        ? "Current Objective:" 
        : "";

        slotObjectives[slot - 1].text = data != null && data.objectiveSaveData != null 
        ? $"{data.objectiveSaveData.objectives[data.objectiveSaveData.currentObjectiveIndex].objectiveName}" 
        : "";
        
        playButtons[slot - 1].gameObject.SetActive(data != null);
        deleteButtons[slot - 1].gameObject.SetActive(data != null);

        newGameButtons[slot - 1].gameObject.SetActive(data == null);
    }

    public void ClearSelectedSave(int slot)
    {
        SaveManager.Instance.ClearSaveData(slot);
        UpdateAllSlots();
    }

    public void PlaySelectedSave(int slot)
    {
        SaveManager.Instance.SetActiveSaveSlot(slot);
        LoadGame(slot);
    }

    public void NewGame(int slot = 0)
    {
        SaveManager.Instance.SetActiveSaveSlot(slot);
        SceneManager.LoadScene(firstLevelName);
        Debug.Log("Starting New Game...");
    }

    private void LoadGame(int slot)
    {
        SaveData data = SaveSystem.Load(slot);

        if (data != null && !string.IsNullOrEmpty(data.lastSceneName))
        {
            SceneManager.LoadScene(data.lastSceneName);
            Debug.Log("Continuing Game From Save...");
            return;
        }
        else
        {
            Debug.LogWarning("No valid save data found. Starting New Game instead.");
            NewGame();
        }
    }
}
