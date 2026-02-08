using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool instanceReady = false;
    public string currentSceneName;
    public int currentSceneIndex;

    // References to child objects
    private GameObject player;
    private GameObject saveManager;
    private GameObject audioManager;
    private GameObject gameOverManager;
    private GameObject mainCanvas;
    private GameObject interactionIconsCanvas;
    private GameObject journalUICanvas;
    private GameObject playerUICanvas;
    private GameObject pauseManager;
    private GameObject dialogueManager;
    private GameObject objectiveManager;
    private GameObject eventSystem;

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

        StartCoroutine(WaitForCopiesToDelete());

        UpdateChildReferences(); // Update references to child objects
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private IEnumerator WaitForCopiesToDelete()
    {
        var otherGameManagers = FindObjectsByType<GameManager>(FindObjectsSortMode.None);
        while (otherGameManagers.Length > 1)
        {
            yield return null; // Wait for the next frame
            otherGameManagers = FindObjectsByType<GameManager>(FindObjectsSortMode.None);
        }
        instanceReady = true; // Set instanceReady to true once all duplicates are deleted
    }

    private void UpdateChildReferences()
    {
        // Find child objects and store references to them
        player = GetComponentInChildren<PlayerController>()?.gameObject;
        saveManager = GetComponentInChildren<SaveManager>()?.gameObject;
        audioManager = GetComponentInChildren<AudioManager>()?.gameObject;
        gameOverManager = GetComponentInChildren<GameOverManager>()?.gameObject;
        mainCanvas = transform.Find("MainCanvas")?.gameObject;
        interactionIconsCanvas = GetComponentInChildren<PopupManager>()?.gameObject;
        journalUICanvas = GetComponentInChildren<Journal>()?.gameObject;
        playerUICanvas = GetComponentInChildren<TimerRingUI>()?.gameObject;
        pauseManager = GetComponentInChildren<PauseManager>()?.gameObject;
        dialogueManager = GetComponentInChildren<DialogueManager>()?.gameObject;
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
            if (player.activeSelf)
                player.SetActive(false);

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

            if (dialogueManager.activeSelf)
                dialogueManager.SetActive(false);
        }
        else
        {
            if (!player.activeSelf)
                player.SetActive(true);

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

            if (!dialogueManager.activeSelf)
                dialogueManager.SetActive(true);
        }

        var playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (playerControllers.Length > 0)
        {
            foreach (var players in playerControllers)
            {
                if (players.gameObject != this.player)
                {
                    players.gameObject.SetActive(false);
                    // If another player instance is found, update the main player reference and destroy the duplicate
                    player.transform.position = players.transform.position;
                    player.transform.rotation = players.transform.rotation;
                    player.transform.localScale = players.transform.localScale; 

                    Destroy(players.gameObject);
                }
            }
        }
    }
}
