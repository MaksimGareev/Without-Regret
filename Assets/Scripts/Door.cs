using System.Collections;
using UnityEditor;
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
    public float interactionPriority => 5f;
    public InteractType interactType => InteractType.Door;

    // Player
    private Transform player;
    [Header("Objective Settings")]
    [Tooltip("Objective that must be COMPLETED to allow the player to interact with this door. If the player has not COMPLETED the linked objective, they will not be able to interact with the door.")]
    public ObjectiveData linkedObjective;
    [Tooltip("If false, the player will be able to interact with this door without needing to complete the linked objective, and the Linked Objective will be ignored. If true, the player must complete the linked objective before they can interact with this door, and the Linked Objective will need to be assigned.")]
    public bool needsObjective = true;


    [Header("Audio Settings")]
    [Tooltip("Sound that will play when the player interacts with the door.")]
    public AudioClip interactSound;
    private AudioSource audioSource; //not sure if needed ,but will keep for now

    private bool isPlayerNear = false;
    private bool isInteracting = false;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

  private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        /*if (player != null && Vector3.Distance(player.position, transform.position) <= interactDistance)
        {
            isPlayerNear = true;

            // if player presses E to enter
            if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Xbox X Button"))
            {
                LoadScene();
            }
        }
        else
        {
            isPlayerNear = false;
        }*/
    }

    public bool CanInteract(GameObject player)
    {
        if (!needsObjective) return true;
        

        var completed = ObjectiveManager.Instance.GetCompletedObjectives();
        return completed.Any(o => o.data == linkedObjective);
    }

    public void OnPlayerInteraction(GameObject player)
    {
        if (interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }
        StartCoroutine(WaitToLoadScene());
    }

    void LoadScene()
    {
        /*if (needsObjective)
        {
            var completeObjectives = ObjectiveManager.Instance.GetCompletedObjectives();
            foreach (var obj in completeObjectives)
            {
                if (obj.data == linkedObjective)
                {
                    StartCoroutine(WaitToLoadScene());
                    return;
                }
                else
                {
                    Debug.Log("You must complete all objectives before moving forward");
                }
            }
        }
        else
        {
            StartCoroutine(WaitToLoadScene());
        }*/
    }

    private IEnumerator WaitToLoadScene()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
        
        yield return new WaitForSeconds(0.1f);

        if (sceneToLoad == null || string.IsNullOrEmpty(sceneToLoad.GetSceneName()))
        {
            Debug.LogError("Scene to load is not set on the door.");
            yield break;
        }

        SceneManager.LoadScene(sceneToLoad.GetSceneName());
    }
}

