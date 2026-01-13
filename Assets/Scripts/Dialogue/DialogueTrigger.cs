using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DialogueTrigger : MonoBehaviour
{
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

    private bool playerInRange = false;
    private Transform player;
    private PlayerControls controls;
    public bool TalkedAlready = false;

    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

    public GameObject enemy;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Interact.performed += ctx => TryInteract();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Player reference
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Dialogue Manager
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (promptUI != null)
            promptUI.SetActive(false);

        enemy.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Distance to player
        float dist = Vector3.Distance(player.position, transform.position);

        // Show/Hide prompt UI
        if (dist <= chatRange)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                if (iconPrefab != null)
                    iconPrefab.SetActive(true);
                if (promptUI != null)
                    promptUI.SetActive(true);
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                if (iconPrefab != null)
                    iconPrefab.SetActive(false);
                if (promptUI != null)
                    promptUI.SetActive(false);
            }
        }

        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null && !playerInRange)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }

    }

    private void TryInteract()
    {
        // Only trigger if player is close enough and not already in dialogue
        if (!playerInRange || PlayerController.DialogueActive)
            return;

        // PlayerController.DialogueActive = true;
        DisablePopupIcon();

        if (promptUI != null)
            promptUI.SetActive(false);

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
            dialogueManager.StartDialogueFromJson(CompleteJsonDialogueFile);

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
            dialogueManager.StartDialogueFromJson(ActiveJsonDialogueFile);
            return;
        }

        // Starting dialogue
        if (dialogueManager != null && jsonDialogueFile != null && TalkedAlready == false)
        {
            dialogueManager.StartDialogueFromJson(jsonDialogueFile);
            
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
        else if (TalkedAlready == true)
        {
            dialogueManager.StartDialogueFromJson(TalkedJsonDialogueFile);

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && TalkedAlready == false)
        {
            if (dialogueManager != null && jsonDialogueFile != null)
            {
                dialogueManager.StartDialogueFromJson(jsonDialogueFile);
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
