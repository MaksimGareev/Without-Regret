using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    public float interactionPriority => 10f;
    public InteractType interactType => InteractType.Dialogue;

    public string NPCName = "Friendly NPC";
    public GameObject promptUI;
    public float chatRange = 3f;

    // Dialogue files
    public TextAsset jsonDialogueFile;
    public TextAsset TalkedJsonDialogueFile;
    public TextAsset CompleteJsonDialogueFile;
    public TextAsset ActiveJsonDialogueFile;
    
    private DialogueManager dialogueManager;

    // objectives the npc is responisble for
    public List<string> objectiveIDYouCareAbout = new List<string>();
    public ObjectiveData linkedObjective;

    // Look at player
    public bool IsMediation = false;
    public float lookSpeed = 5f;
    public bool isLookingAtPlayer = false;

    // wandering
    private NpcMovement npcWander;

    private bool playerInRange = false;
    private Transform player;
    //private PlayerControls controls;
    public bool TalkedAlready = false;

    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

    public GameObject enemy;
    public bool focusCameraOnTrigger = false;

    private void Awake()
    {
        //controls = new PlayerControls();

       // controls.Player.Interact.performed += ctx => TryInteract();
    }

   // private void OnEnable() => controls.Enable();
   // private void OnDisable() => controls.Disable();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Player reference
        player = GameObject.FindGameObjectWithTag("Player").transform;

        npcWander = GetComponent<NpcMovement>();

        // Dialogue Manager
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (NPCName == "Story")
        {
            shouldShowIcon = false;
            DisablePopupIcon();
        }

        if (promptUI != null)
            promptUI.SetActive(false);

        if (enemy != null)
            enemy.SetActive(false);
    }

    public void OnPlayerInteraction(GameObject player)
    {
        Irene irene = GetComponent<Irene>();
        if (irene != null && irene.IsFollowing)
        {
            Debug.Log("Irene Cannot talk to Irene while she is following");
            return;
        }

        TryInteract();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null && !playerInRange)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }*/

        if (isLookingAtPlayer && !IsMediation)
        {
            LookAtPlayer();
        }
    }

    private void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 Direction = player.position - transform.position;
        Direction.y = 0f; // Prevent tilting

        if (Direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(Direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }

    public void StopLookingAtPlayer()
    {
        isLookingAtPlayer = false;
    }

    private void TryInteract()
    {
        if (DialogueManager.DialogueIsActive)
        {
            Debug.Log("Dialogue already active, ignoring interaction.");
            return;
        }

        Debug.Log("DialogueTrigger: TryInteract called");

        if (ButtonIcons.Instance != null)
        {
            ButtonIcons.Instance.Clear();
        }

        // stop wandering when dialogue starts
        if (npcWander != null)
        {
            npcWander.StopWandering();
        }

        // PlayerController.DialogueActive = true;
        DisablePopupIcon();

        if (promptUI != null)
            promptUI.SetActive(false);

        isLookingAtPlayer = true;

        // check if the player completed any objectives the npc is responsible for
        bool allCompleted = true;
        foreach (string objectiveID in objectiveIDYouCareAbout)
        {
            if (!ObjectiveManager.Instance.IsObjectiveCompleted(objectiveID))
            {
                allCompleted = false;
                break;
            }
        }
        // if all are completed play completed dialogue
        if (allCompleted && CompleteJsonDialogueFile != null && TalkedAlready == true)
        {
            Debug.Log("DialogueTrigger: Starting dialogue from JSON");
            dialogueManager.StartDialogueFromJson(CompleteJsonDialogueFile, this);

            if (ObjectiveManager.Instance != null && linkedObjective != null)
            {
                if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
                {
                    ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                }
            }
            
            return;
        }

        // check if the player has an active objective the npc is responsible for
        bool anyActive = false;
        foreach (string objectiveID in objectiveIDYouCareAbout)
        {
            if (ObjectiveManager.Instance.IsObjectiveActive(objectiveID) && TalkedAlready == true)
            {
                anyActive = true;
                break;
            }
        }
        // if any are active play active dialogue
        if (anyActive && ActiveJsonDialogueFile != null)
        {
            Debug.Log("DialogueTrigger: Starting dialogue from JSON");
            dialogueManager.StartDialogueFromJson(ActiveJsonDialogueFile, this);
            return;
        }

        // Starting dialogue
        if (dialogueManager != null && jsonDialogueFile != null && TalkedAlready == false)
        {
            Debug.Log("DialogueTrigger: Starting dialogue from JSON");
            dialogueManager.StartDialogueFromJson(jsonDialogueFile, this);
            
            // Add Progress to objective if there is one to add to, (Talking to irene completes the "talk to irene" objective)
            if (ObjectiveManager.Instance != null && linkedObjective != null)
            {
                if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
                {
                    ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                }
            }

            TalkedAlready = true;
        }
        // dialogue if the npc has already been talked to and hasn't started any objectives from the npc
        else if (TalkedAlready == true && TalkedJsonDialogueFile != null)
        {
            Debug.Log("DialogueTrigger: Starting dialogue from JSON");
            dialogueManager.StartDialogueFromJson(TalkedJsonDialogueFile, this);

            if (ObjectiveManager.Instance != null && linkedObjective != null)
            {
                if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
                {
                    ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                }
            }
        }

        // **Activate the enemy when dialogue is triggered**
        if (enemy != null)
        {
            enemy.SetActive(true);
            Debug.Log($"{enemy.name} activated by {NPCName}");
        }

    }

    public void ResumeWandering()
    {
        if(npcWander != null)
        {
            npcWander.ResumeWandering();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && TalkedAlready == false)
        {
            if (dialogueManager != null && jsonDialogueFile != null)
            {
                dialogueManager.StartDialogueFromJson(jsonDialogueFile, this);
            }
            TalkedAlready = true;
           /* if (this.CompareTag("Spawner") && enemy != null)
            {
                enemy.SetActive(true);
            }*/
        }
    }

    public void EnablePopupIcon()
    {
        if (NPCName == "Story") return;

        if (popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
            shouldShowIcon = true;
        }
    }

    public void DisablePopupIcon()
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
            popupInstance = null;
            shouldShowIcon = false;
        }
    }
}
