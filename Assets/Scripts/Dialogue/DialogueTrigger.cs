using System.Collections;
using System.Collections.Generic;
//using UnityEditor.EditorTools;
using UnityEngine;
//using UnityEngine.AI;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Animation")]
    public Animator animator;
    public bool isTalking = false;
    private Coroutine talkRoutine;

    private Animator playerAnimator;
    private Coroutine playerTalkRoutine;
    public float interactionPriority => 10f;
    public InteractType interactType => InteractType.Dialogue;

    [Header("Name and chat Range")]
    [Tooltip("The name of the trigger (in the dialogue manager this is changed to the Speaker variable within the JSON, this can be used to trigger specific events)")]
    public string NPCName = "Friendly NPC";
    [Tooltip("How far away the player must be to interact with the NPC")]
    public float chatRange = 3f;
    [SerializeField, Tooltip("The position the player moves to when interacting with the NPC, if null the player will not move from the point of interaction")]
    private Transform playerMovePoint;

    [Header("Json dialogue files")]
    [Tooltip("Json file that will be loaded on the players first interaction with a NPC or used for the story dialogue trigger")]
    public TextAsset jsonDialogueFile;
    [Tooltip("Json file that will be loaded if the player interacts with the NPC and they do not have a objective they care about")]
    public TextAsset TalkedJsonDialogueFile;
    [Tooltip("Json file that will be loaded when the player interacts with the NPC after completing the objective they care about")]
    public TextAsset CompleteJsonDialogueFile;
    [Tooltip("Json file that will be loaded when the player interacts with the NPC with the objective they care about being active")]
    public TextAsset ActiveJsonDialogueFile;
    
    private DialogueManager dialogueManager;

    [Header("Objective data the NPC is responsible for")]
    [Tooltip("This is a list of objective IDs that the NPC cares about and must be completed to trigger the complete Json file and progress the story")]
    public List<string> objectiveIDYouCareAbout = new List<string>();
    [Tooltip("Objective data that will be progressed when talking to the NPC such as talking to Irene to complete the meet irene objective")]
    public ObjectiveData linkedObjective;

    [Header("Looking at player variables")]
    [Tooltip("A bool that is used to identify if a dialogue interaction is a mediation making the NPC not look at the player")]
    public bool IsMediation = false;
    [Tooltip("How fast the NPC will look towards the player after engaging in dialogue")]
    public float lookSpeed = 5f;
    public bool isLookingAtPlayer = false;

    // wandering
    private NpcMovement npcWander;

    private bool playerInRange = false;
    private Transform player;
    //private PlayerControls controls;
    [Tooltip("A bool identifying if the player has talked to this NPC already")]
    public bool TalkedAlready = false;

    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

    public GameObject enemy;
    [Tooltip("A bool checking if the camera will zoom in on the NPC (should be false for mediation or can be included if design request differently")]
    public bool focusCameraOnTrigger = false;
    [Header("Reward ItemData")]
    [Tooltip("Optional item to give the player upon completing the dialogue. Will only be given once.")]
    public ItemData RewardItem;

    public enum DialogueTriggerType
    {
        NPC,
        Spawn,
        Story
    }

    [SerializeField]
    private DialogueTriggerType triggerType = DialogueTriggerType.NPC;

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
        if (playerAnimator == null)
        {
            playerAnimator = player.GetComponentInChildren<Animator>();
            Debug.Log("Player Animator found!");
        }
        else
            Debug.Log("Player Animator not found!");

        npcWander = GetComponent<NpcMovement>();

        // Dialogue Manager
        dialogueManager = FindAnyObjectByType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager not found in the scene. Please add one.");
        }

        if (triggerType == DialogueTriggerType.Story)
        {
            shouldShowIcon = false;
            DisablePopupIcon();
        }

        if (enemy != null)
            enemy.SetActive(false);
    }

    public bool CanInteract(GameObject player)
    {
        if (DialogueManager.DialogueIsActive)
            return false;

        if (triggerType == DialogueTriggerType.Story)
            return false;

        if (TalkedAlready && TalkedJsonDialogueFile == null)
            return false;

        return true;
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

        isTalking = true;
        SafeSetBool("isTalking", isTalking);
        talkRoutine ??= StartCoroutine(TalkAnimationCycle());

        Vector3 Direction = player.position - transform.position;
        Direction.y = 0f; // Prevent tilting

        if (Direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(Direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }

    private IEnumerator MoveAndLookAt(Transform mover, Transform lookat)
    {
        // Move a transform to the specified point, then rotate it toward lookat
        while (mover.transform != playerMovePoint)
        {
            mover.position = Vector3.MoveTowards(mover.position, playerMovePoint.position, 2f * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }

        Vector3 direction = lookat.position - mover.position; // get direction from mover to the lookat target
        direction.y = 0f; // Prevent tilting

        if (direction.sqrMagnitude < 0.01f) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        while (Quaternion.Angle(mover.rotation, targetRotation) > 0.5f)
        {
            mover.rotation = Quaternion.Slerp(mover.rotation, targetRotation, lookSpeed * Time.deltaTime);
        }
    }

    public void StopLookingAtPlayer()
    {
        isTalking = false;
        SafeSetBool("isTalking", isTalking);
        SafeSetBool("Talk1", isTalking);
        SafeSetBool("Talk2", isTalking);

        if(talkRoutine != null)
        {
            StopCoroutine(talkRoutine);
            talkRoutine = null;
        }

        StopPlayerTalking();

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

            if (triggerType == DialogueTriggerType.NPC)
            {
                ButtonIcons.Instance.Highlight(interactType);
            }
        }

        // stop wandering when dialogue starts
        if (npcWander != null)
        {
            npcWander.StopWandering();
        }

        // PlayerController.DialogueActive = true;
        DisablePopupIcon();

        isLookingAtPlayer = true;
        StartPlayerTalking();
        if (playerMovePoint != null)
            StartCoroutine(MoveAndLookAt(player, this.transform));

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
            GameManager.Instance.dialogueManager.StartDialogueFromJson(CompleteJsonDialogueFile, this);

            // if (ObjectiveManager.Instance != null && linkedObjective != null)
            // {
            //     if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
            //     {
            //         ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            //     }
            // }
            
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
            GameManager.Instance.dialogueManager.StartDialogueFromJson(ActiveJsonDialogueFile, this);
            return;
        }

        // Starting dialogue
        if (GameManager.Instance.dialogueManager != null && jsonDialogueFile != null && TalkedAlready == false)
        {
            Debug.Log("DialogueTrigger: Starting dialogue from JSON");
            GameManager.Instance.dialogueManager.StartDialogueFromJson(jsonDialogueFile, this);
            
            // Add Progress to objective if there is one to add to, (Talking to irene completes the "talk to irene" objective)
            // if (ObjectiveManager.Instance != null && linkedObjective != null)
            // {
            //     if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
            //     {
            //         ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            //     }
            // }

            TalkedAlready = true;
        }
        // dialogue if the npc has already been talked to and hasn't started any objectives from the npc
        else if (TalkedAlready == true && TalkedJsonDialogueFile != null)
        {
            Debug.Log("DialogueTrigger: Starting dialogue from JSON");
            GameManager.Instance.dialogueManager.StartDialogueFromJson(TalkedJsonDialogueFile, this);

            // if (ObjectiveManager.Instance != null && linkedObjective != null)
            // {
            //     if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
            //     {
            //         ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            //     }
            // }
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
        StartDialogueFromTrigger(other);
        //StartCoroutine(WaitForGameManagerReady(other));
    }

    private IEnumerator WaitForGameManagerReady(Collider other)
    {
        if (GameManager.Instance != null)
        {
            while (!GameManager.Instance.instanceReady)
            {
                yield return null; // Wait for the next frame
            }
        }
        
        StartDialogueFromTrigger(other);
    }

    private void StartDialogueFromTrigger(Collider other)
    {
        if (triggerType == DialogueTriggerType.NPC || other == null) return;

        if (other.CompareTag("Player") && !TalkedAlready)
        {
            if (this.CompareTag("Spawner"))
            {
                enemy.SetActive(true);
            }

            // if (dialogueManager == null)
            // {
            //     dialogueManager = FindObjectOfType<DialogueManager>();
            // }

            if (GameManager.Instance.dialogueManager != null && jsonDialogueFile != null)
            {
                GameManager.Instance.dialogueManager.StartDialogueFromJson(jsonDialogueFile, this);

                // // Add Progress to objective if there is one to add to, (Talking to irene completes the "talk to irene" objective)
                // if (ObjectiveManager.Instance != null && linkedObjective != null)
                // {
                //     if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
                //     {
                //         ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                //     }
                // }
            }

            TalkedAlready = true;
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

    private void StartPlayerTalking()
    {
        if (playerAnimator == null)
            return;
        Debug.Log("Started Talking");
        playerAnimator.SetBool("isTalking", true);

        playerTalkRoutine ??= StartCoroutine(PlayerTalkAnimationCycle());
    }

    public void StartPlayerThinking()
    {
        if (playerAnimator == null)
            return;

        if (playerTalkRoutine != null)
        {
            StopCoroutine(playerTalkRoutine);
            playerTalkRoutine = null;
        }
        playerAnimator.SetBool("Talk1", false);
        playerAnimator.SetBool("Talk2", false);

        playerAnimator.SetBool("Think", true);
    }

    public void StopPlayerThinking()
    {
        if (playerAnimator == null)
            return;

        if (playerTalkRoutine != null)
        {
            StopCoroutine(playerTalkRoutine);
            playerTalkRoutine = null;
        }

        playerAnimator.SetBool("Think", false);
        StartPlayerTalking();
    }

    private void StopPlayerTalking()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.SetBool("isTalking", false);
        playerAnimator.SetBool("Talk1", false);
        playerAnimator.SetBool("Talk2", false);

        if (playerTalkRoutine != null)
        {
            StopCoroutine(playerTalkRoutine);
            playerTalkRoutine = null;
        }
    }

    IEnumerator PlayerTalkAnimationCycle()
    {
        if (playerAnimator == null)
            yield break;
        Debug.Log("Started Talk animation cycle");
        //if (playerAnimator.GetBool("isThinking"))
        //    yield break;
        while (playerAnimator.GetBool("isTalking"))
        {
            playerAnimator.SetBool("Talk1", true);
            playerAnimator.SetBool("Talk2", false);
            yield return new WaitForSeconds(5.0f);

            playerAnimator.SetBool("Talk1", false);
            playerAnimator.SetBool("Talk2", true);
            yield return new WaitForSeconds(5.0f);
        }
    }

    IEnumerator TalkAnimationCycle() //currently cycles back and forth between both talk animations while speaking
    {
        if (animator == null)
            yield break;
        while (isTalking)
        {
            SafeSetBool("Talk2", false);
            SafeSetBool("Talk1", true);
            yield return new WaitForSeconds(5.0f);
            SafeSetBool("Talk1", false);
            SafeSetBool("Talk2", true);
            yield return new WaitForSeconds(5.0f);
        }
    }

    private void SafeSetBool(string parameter, bool value) //this is used in place of animator.setbool so that it can function correctly on non-character objects
    {
        if (animator != null)
            animator.SetBool(parameter, value);
    }
}
