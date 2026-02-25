using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private MainMenu mainMenu;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI[] slotTexts = new TextMeshProUGUI[3];
    [SerializeField] private TextMeshProUGUI[] slotObjectives = new TextMeshProUGUI[3];
    [SerializeField] private TextMeshProUGUI[] slotStatus = new TextMeshProUGUI[3];
    
    [Header("Buttons")]
    [SerializeField] public Button[] playButtons = new Button[3];
    [SerializeField] private Button[] deleteButtons = new Button[3];
    [SerializeField] public Button[] newGameButtons = new Button[3];

    [Header("FirstLevelReference")]
    [SerializeField] private SceneReference firstScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetUpListeners();
    }

    private void OnEnable()
    {
        SetUpListeners();
        UpdateAllSlots();
    }

    private void UpdateAllSlots()
    {
        for (int slot = 1; slot <= 3; slot++)
        {
            try
            {
                Debug.Log($"Loading Save Slot {slot} Data...");
                SaveData data = SaveSystem.Load(slot);
                UpdateSlotInfo(slot, data);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to Update slot {slot}: {ex.Message}");
                UpdateSlotInfo(slot, null);
            }
        }

        if (mainMenu != null)
        {
            mainMenu.SelectSaveMenuButton();
        }
    }

    private void SetUpListeners()
    {
        // Set up button listeners for each slot, linked to proper numbers for save system
        playButtons[0].onClick.AddListener(() => PlaySelectedSave(1));
        deleteButtons[0].onClick.AddListener(() => ConfirmBeforeDelete(1));
        newGameButtons[0].onClick.AddListener(() => NewGame(1));

        playButtons[1].onClick.AddListener(() => PlaySelectedSave(2));
        deleteButtons[1].onClick.AddListener(() => ConfirmBeforeDelete(2));
        newGameButtons[1].onClick.AddListener(() => NewGame(2));
        
        playButtons[2].onClick.AddListener(() => PlaySelectedSave(3));
        deleteButtons[2].onClick.AddListener(() => ConfirmBeforeDelete(3));
        newGameButtons[2].onClick.AddListener(() => NewGame(3));
    }

    private void RemoveListeners()
    {
        for (int i = 0; i < 3; i++)
        {
            playButtons[i].onClick.RemoveAllListeners();
            deleteButtons[i].onClick.RemoveAllListeners();
            newGameButtons[i].onClick.RemoveAllListeners();
        }
    }

    public void UpdateSlotInfo(int slot, SaveData data)
    {
        Debug.Log($"Updating Slot {slot} Info");
        
        if (slot < 1 || slot > 3)
        {
            Debug.LogError("Invalid slot number: " + slot);
            return;
        }

        if (data == null)
        {
            Debug.Log($"No save data found for slot {slot}");
        }
        else 
        {
            Debug.Log($"Save data found for slot {slot}: Last Scene - {data.lastSceneName}");
        }

        slotTexts[slot - 1].text = data != null 
        ? $"Save {slot}" 
        : "Empty";

        slotStatus[slot - 1].text = data != null 
        ? "Current Objective:" 
        : "";

        string objectiveName = "";

        if (data != null && data.objectiveSaveData != null && data.objectiveSaveData.objectives != null)
        {
            var list = data.objectiveSaveData.objectives;
            objectiveName = (list.Count > 0 && list[0] != null) ? list[0].objectiveName : "";
        }

        slotObjectives[slot - 1].text = objectiveName;
        
        playButtons[slot - 1].gameObject.SetActive(data != null);
        deleteButtons[slot - 1].gameObject.SetActive(data != null);

        newGameButtons[slot - 1].gameObject.SetActive(data == null);
    }

    private void ConfirmBeforeDelete(int slot)
    {
        confirmationPanel.SetActive(true);

        DisableAllButtons();

        ConfirmationUI confirmationUI = confirmationPanel.GetComponent<ConfirmationUI>();
        confirmationUI.ConfirmTask(ConfirmationType.DeleteSave, 
            () => 
            {
                // Confirm action
                ClearSelectedSave(slot);
                confirmationPanel.SetActive(false);
                EnableAllButtons();
                mainMenu.SelectSaveMenuButton();
            },
            () => 
            {
                // Cancel action
                confirmationPanel.SetActive(false);
                EnableAllButtons();
                mainMenu.SelectSaveMenuButton();
            });
    }

    private void ClearSelectedSave(int slot)
    {
        SaveManager.Instance.ClearSaveData(slot);
        UpdateAllSlots();
    }

    private void PlaySelectedSave(int slot)
    {
        SaveManager.Instance.SetActiveSaveSlot(slot);
        LoadGame(slot);
    }

    private void NewGame(int slot = 1)
    {
        SaveManager.Instance.SetActiveSaveSlot(slot);
        SaveManager.Instance.LoadGame(slot);
        SceneManager.LoadScene(firstScene.GetSceneName());
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

    private void DisableAllButtons()
    {
        for (int i = 0; i < playButtons.Count(); i++)
        {
            playButtons[i].interactable = false;
            deleteButtons[i].interactable = false;
            newGameButtons[i].interactable = false;
        }
    }

    private void EnableAllButtons()
    {
        for (int i = 0; i < playButtons.Count(); i++)
        {
            playButtons[i].interactable = true;
            deleteButtons[i].interactable = true;
            newGameButtons[i].interactable = true;
        }
    }
    
    private void OnDisable()
    {
        RemoveListeners();
    }
}
