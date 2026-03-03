using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The GameManager is a singleton that persists across scenes and manages references to important child objects
// It allows other scripts to reference these child objects without all needing to individually find the objects themselves.
// It also ensures that each scene has the appropriate UI, Managers, and other necessary objects without Designer input.
// It also handles toggling the active state of child objects based on the current scene
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance
    [HideInInspector] public bool instanceReady = false; // Flag to let other scripts know when it is appropriate to acces the GameManager
    public string currentSceneName;
    public int currentSceneIndex;

    // References to child objects, for the most part the names match the object names in heirarchy for assignment
    [HideInInspector] public GameObject saveManager;
    public GameObject saveCanvas;
    [HideInInspector] public GameObject audioManager;
    [HideInInspector] public GameObject gameOverManager;
    public GameObject gameOverCanvas;
    [HideInInspector] public GameObject mainCanvas;
    public Slider staminaSlider;
    public Slider throwingSlider;
    public Slider possessionSlider;
    public Slider floatingSlider;
    public RectTransform floatTargetArea;
    public Slider floatCooldown;
    public Image staminaFill;
    public GameObject InventoryUI;
    public GameObject LockPickUI;
    public RectTransform inventoryRectTransform;
    public InventoryUIController inventoryInteractingScript;
    public TextMeshProUGUI inventoryPopupText;
    [HideInInspector] public GameObject interactionIconsCanvas;
    [HideInInspector] public GameObject journalUICanvas;
    public GameObject journalUI;
    [HideInInspector] public GameObject playerUICanvas;
    [HideInInspector] public GameObject pauseManager;
    public GameObject pauseMenu;
    [HideInInspector] public DialogueManager dialogueManager;
    public GameObject dialoguePanel;
    [HideInInspector] public GameObject objectiveManager;
    public ObjectiveCanvas objectiveCanvas;
    public GameObject objectivePanel;
    [HideInInspector] public GameObject eventSystem;

    [HideInInspector] public NewDialogueManager newDialogueManager;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate GameManager if another instance already exists
        }

        UpdateChildReferences(); // Update references to child objects
    }

    private void OnEnable()
    {
        // Subscribe to scene change events
        SceneManager.activeSceneChanged += OnSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene change events
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitForCopiesToDelete());
    }

    // Waits until all duplicate GameManager instances are deleted, marks the instance as ready for other scripts to access
    private IEnumerator WaitForCopiesToDelete()
    {
        yield return null; // Wait for the next frame to ensure all objects are loaded
        yield return new WaitForEndOfFrame();

        var otherGameManagers = FindObjectsByType<GameManager>(FindObjectsSortMode.None);
        while (otherGameManagers.Length > 1)
        {
            yield return null;
            otherGameManagers = FindObjectsByType<GameManager>(FindObjectsSortMode.None);
        }

        instanceReady = true;

        // Load game data if not in main menu
        if (SaveManager.Instance != null && SceneManager.GetActiveScene().name != "MainMenu")
        {
            SaveManager.Instance.LoadGame(SaveSystem.activeSaveSlot);
        }
    }

    // Finds and updates references to child objects
    // Currently only done once on GameManager's Awake
    private void UpdateChildReferences()
    {
        saveManager = GetComponentInChildren<SaveManager>()?.gameObject;
        audioManager = GetComponentInChildren<AudioManager>()?.gameObject;
        gameOverManager = GetComponentInChildren<GameOverManager>()?.gameObject;
        mainCanvas = transform.Find("MainCanvas")?.gameObject;
        interactionIconsCanvas = GetComponentInChildren<PopupManager>()?.gameObject;
        journalUICanvas = GetComponentInChildren<Journal>()?.gameObject;
        playerUICanvas = GetComponentInChildren<TimerRingUI>()?.gameObject;
        pauseManager = GetComponentInChildren<PauseManager>()?.gameObject;
        dialogueManager = GetComponentInChildren<DialogueManager>();
        objectiveManager = GetComponentInChildren<ObjectiveManager>()?.gameObject;
        eventSystem = GetComponentInChildren<EventSystem>()?.gameObject;
    }

    // Called from the scene change event subscribed to above
    // Updates info about current scene and toggles child objects appropriately
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        instanceReady = false;
        StartCoroutine(WaitForCopiesToDelete());

        // Debug.Log("Scene changed to: " + newScene.name);
        currentSceneName = newScene.name;
        currentSceneIndex = newScene.buildIndex;

        ToggleChildrenActive();
    }

    // Disables child objects that aren't needed when in the main menu, enables them in other levels if not already
    private void ToggleChildrenActive()
    {
        if (currentSceneName == "MainMenu")
        {
            if (gameOverManager.activeSelf)
                gameOverManager.SetActive(false);

            if (mainCanvas.activeSelf)
                mainCanvas.SetActive(false);

            if (interactionIconsCanvas.activeSelf)
                interactionIconsCanvas.SetActive(false);

            if (journalUICanvas.activeSelf)
                journalUICanvas.SetActive(false);

            if (playerUICanvas.activeSelf)
                playerUICanvas.SetActive(false);

            if (pauseManager.activeSelf)
                pauseManager.SetActive(false);

            if (dialogueManager.gameObject.activeSelf)
                dialogueManager.gameObject.SetActive(false);
        }
        else
        {
            if (!gameOverManager.activeSelf)
                gameOverManager.SetActive(true);

            if (!mainCanvas.activeSelf)
                mainCanvas.SetActive(true);

            if (!interactionIconsCanvas.activeSelf)
                interactionIconsCanvas.SetActive(true);

            if (!journalUICanvas.activeSelf)
                journalUICanvas.SetActive(true);

            if (!playerUICanvas.activeSelf)
                playerUICanvas.SetActive(true);

            if (!pauseManager.activeSelf)
                pauseManager.SetActive(true);

            if (!dialogueManager.gameObject.activeSelf)
                dialogueManager.gameObject.SetActive(true);
        }
    }
}
