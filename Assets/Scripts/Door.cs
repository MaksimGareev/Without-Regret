using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [Tooltip("The scene that this door will load when the player interacts with it. Drag and drop the scene asset from the project window into this field.")]
    public SceneReference sceneToLoad;
    
    [Tooltip("Distance at which the player can interact with the door.")]
    public float interactDistance = 5f;
    
    [Tooltip("If assigned, this cutscene will play before loading the next level.")]
    public CutsceneData cutsceneToPlay;
    
    public float interactionPriority => 5f;
    public InteractType interactType => InteractType.Door;

    // Player
    private Transform player;
    private Animator animator;
    private CharacterSwap characterSwap;
    [Header("Objective Settings")]
    [Tooltip("Objective that must be ACTIVE to allow the player to interact with this door. If the player has not ACTIVE the linked objective, they will not be able to interact with the door.")]
    public ObjectiveData linkedObjective;

    [Tooltip("If false, the player will be able to interact with this door without needing to complete the linked objective, and the Linked Objective will be ignored. If true, the player must complete the linked objective before they can interact with this door, and the Linked Objective will need to be assigned.")]
    public bool needsObjective = true;

    [Tooltip("If true, interacting with the door will add progress to the linked objective. If false, interacting with the door will not add progress to the linked objective, but will still be locked based on the needs objective toggle.")]
    public bool addProgress = false;

    [Header("Audio Settings")]
    [Tooltip("Sound that will play when the player interacts with the door.")]
    public AudioClip interactSound;
    private AudioSource audioSource; //not sure if needed ,but will keep for now

    private bool hasInteracted = false;

    private void Awake()
    {
        characterSwap = FindFirstObjectByType<CharacterSwap>();

        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = player.GetComponentInChildren<Animator>();
    }

    public bool CanInteract(GameObject player)
    {
        if (sceneToLoad == null || string.IsNullOrEmpty(sceneToLoad.GetSceneName())) return false;
        
        if (!needsObjective) return true;

        var active = ObjectiveManager.Instance.GetActiveObjectives();
        return active.Any(o => o.data == linkedObjective);
    }

    public void OnPlayerInteraction(GameObject player)
    {
        // prevent spamming
        if (hasInteracted) return;

        hasInteracted = true;

        if (sceneToLoad == null || string.IsNullOrEmpty(sceneToLoad.GetSceneName()))
        {
            hasInteracted = false;
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger("DoorOpen");
        }
        if (interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }
        
        StartCoroutine(WaitToLoadScene());
    }

    private IEnumerator WaitToLoadScene()
    {
        if (sceneToLoad == null || string.IsNullOrEmpty(sceneToLoad.GetSceneName()))
        {
            Debug.LogError("Scene to load is not set on the door.");
            yield break;
        }
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        if (addProgress && ObjectiveManager.Instance != null && needsObjective && linkedObjective != null && ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
        {
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
        }
        
        yield return new WaitForSeconds(0.1f);

        if (GameManager.Instance != null && GameManager.Instance.sceneLoadManager != null)
        {
            GameManager.Instance.sceneLoadManager.LoadScene(sceneToLoad.GetSceneName(), cutsceneToPlay);
        }
        else
        {
            Debug.LogError("SceneLoadManager reference is missing in the GameManager. Loading scene directly without fade transition.");
            SceneManager.LoadScene(sceneToLoad.GetSceneName());
        }        
    }

    void UpdateAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

}

