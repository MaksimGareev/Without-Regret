using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [HideInInspector] public bool instanceReady = false;
    public string currentSceneName;
    public int currentSceneIndex;

    // References to child objects
    //[HideInInspector] public GameObject player;
    [HideInInspector] public GameObject saveManager;
    [HideInInspector] public GameObject audioManager;
    [HideInInspector] public GameObject gameOverManager;
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
    [HideInInspector] public GameObject interactionIconsCanvas;
    [HideInInspector] public GameObject journalUICanvas;
    [HideInInspector] public GameObject playerUICanvas;
    [HideInInspector] public GameObject pauseManager;
    [HideInInspector] public DialogueManager dialogueManager;
    [HideInInspector] public GameObject objectiveManager;
    [HideInInspector] public GameObject eventSystem;

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
        SceneManager.activeSceneChanged += OnSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitForCopiesToDelete());
        //UpdateChildReferences();
    }

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

        if (SaveManager.Instance != null && SceneManager.GetActiveScene().name != "MainMenu")
        {
            SaveManager.Instance.LoadGame(SaveSystem.activeSaveSlot);
        }
    }

    private void UpdateChildReferences()
    {
        // Find child objects and store references to them
        //player = GetComponentInChildren<PlayerController>()?.gameObject;
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

    // Know what scene the player is in
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        instanceReady = false;
        StartCoroutine(WaitForCopiesToDelete());

        Debug.Log("Scene changed to: " + newScene.name);
        currentSceneName = newScene.name;
        currentSceneIndex = newScene.buildIndex;

        //UpdateChildReferences(); // Update references to child objects when the scene changes

        if (currentSceneName == "MainMenu")
        {
            // if (player.activeSelf)
            //     player.SetActive(false);

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
            // if (!player.activeSelf)
            //     player.SetActive(true);

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

        // var players = GameObject.FindGameObjectsWithTag("Player");
        // while (players.Length > 1)
        // {
        //     //yield return null;
        //     players = GameObject.FindGameObjectsWithTag("Player");

        //     foreach (var otherPlayer in players)
        //     {
        //         if (otherPlayer.gameObject != this.player)
        //         {
        //             // If another player instance is found, update the main player reference and destroy the duplicate
        //             otherPlayer.gameObject.SetActive(false);

        //             var data = SaveSystem.Load(SaveSystem.activeSaveSlot);

        //             bool hasTransform = 
        //             data != null && 
        //             data.playerSaveData != null &&
        //             data.playerSaveData.TryGetPlayerTransform(currentSceneName, out float[] pos, out float[] rot);

        //             if (!hasTransform)
        //             {
        //                 this.player.transform.position = otherPlayer.transform.position;
        //                 this.player.transform.rotation = otherPlayer.transform.rotation;
        //                 this.player.transform.localScale = otherPlayer.transform.localScale; 
        //             }
        //             this.player.SetActive(true);
        //             Destroy(otherPlayer.gameObject);
        //         }
        //     }
        // }
    }
}
