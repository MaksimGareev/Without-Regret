using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class NewDialogueTrigger : MonoBehaviour, IInteractable
{

    [Header("Interaction")]
    public float interactionPriority => 10f;
    public InteractType interactType => InteractType.Dialogue;

    [Header("Animation")]
    public Animator animator;
    public bool isTalking = false;
    private Coroutine talkRoutine;
    [Header("Chime Animation")]
    public Animator chimeAnimator;
    public bool chimeActive = false;
    public Chime chimeScript;

    private Animator playerAnimator;
    private Coroutine playerTalkRoutine;

    [Header("Face Animation")]
    public FaceHandler faceHandler;

    [Header("Dialogue Files")]
    [Tooltip("Scriptable dialogue object that will be loaded on the players first interaction with a NPC or used for the story dialogue trigger")]
    public NewDialogueData startDialogueFile;
    [Tooltip("Scriptable dialogue object that will be loaded if the player interacts with the NPC and they do not have a objective they care about")]
    public NewDialogueData talkedDialogueFile;
    [Tooltip("Scriptable dialogue object that will be loaded when the player interacts with the NPC with the objective they care about being active")]
    public NewDialogueData taskActiveDialogueFile;
    [Tooltip("Scriptable dialogue object that will be loaded when the player interacts with the NPC after completing the objective they care about")]
    public NewDialogueData taskCompleteDialogueFile;

    private NewDialogueManager dialogueManager;

    [Header("Name and chat Range")]
    [Tooltip("The name of the trigger (in the dialogue manager this is changed to the Speaker variable within the scriptable object, this can be used to trigger specific events)")]
    public string NPCName = "Friendly NPC";
    [Tooltip("How far away the player must be to interact with the NPC")]
    public float chatRange = 3f;

    [Header("Objectives")]
    public List<string> objectiveIDsYouCareAbout = new List<string>();
    public ObjectiveData linkedObjective;

    [Header("Camera")]
    public bool focusCameraOnTrigger = true;

    [Header("Movement")]
    [SerializeField] private Transform playerMovePoint;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lookSpeed = 6f;

    [Tooltip("A bool that is used to identify if a dialogue interaction is a mediation making the NPC not look at the player")]
    public bool IsMediation = false;

    // wandering
    private NpcMovement npcWander;
    private NavMeshAgent agent;
    public bool hasTalked;
    public bool isLookingAtPlayer;
    private Transform player;

    private bool npcInitiationStarted = false;

    public GameObject enemy;

    private PlayerController playerController;

    [Header("Reward ItemData")]
    [Tooltip("Optional item to give the player upon completing the dialogue. Will only be given once.")]
    public ItemData RewardItem;
    private bool rewardGiven = false;

    public enum DialogueTriggerType
    {
        NPC,
        Spawn,
        Story,
        NPCInitiated
    }

    [SerializeField]
    private DialogueTriggerType triggerType = DialogueTriggerType.NPC;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("Player not found in Scene");
        }

        if (playerAnimator == null)
        {
            playerAnimator = player.GetComponentInChildren<Animator>();
            Debug.Log("Player Animator found!");
        }
        else
            Debug.Log("Player Animator not found!");

        //Finding chime + animator
        GameObject chime = GameObject.FindWithTag("Chime");
        if (chime != null)
        {
            chimeScript = chime.GetComponent<Chime>();
            chimeAnimator = chime.GetComponentInChildren<Animator>();

            chimeActive = true;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent missing on NPC");
        }

        faceHandler = gameObject.GetComponentInChildren<FaceHandler>();

        npcWander = GetComponent<NpcMovement>();

        Debug.Log("Implements IInteractable: " + (this is IInteractable));

        // Dialogue Manager
        dialogueManager = FindAnyObjectByType<NewDialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager not found in the scene. Please add one.");
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        if (triggerType == DialogueTriggerType.Story || triggerType == DialogueTriggerType.NPCInitiated || triggerType == DialogueTriggerType.Spawn)
        {
            return false;
        }

        if (NewDialogueManager.Instance == null)
        {
            Debug.Log("No Dialogue Manager found!");
            return false;
        }

        if (NewDialogueManager.Instance.DialogueIsActive)
        {
            Debug.Log("Dialogue already active!");
            return false;
        }

        return true;
    }

    private void CheckNPCInitiatedDialogue()
    {
        if (triggerType != DialogueTriggerType.NPCInitiated) return;

        if (npcInitiationStarted) return;

        if (hasTalked) return;

        if (player == null) return;

        if (NewDialogueManager.Instance.DialogueIsActive) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= chatRange)
        {
            npcInitiationStarted = true;
            StartCoroutine(NPCWalkToPlayerAndTalk());
        }
    }

    private IEnumerator NPCWalkToPlayerAndTalk()
    {
        hasTalked = true;

        npcWander?.StopWandering();

        if (agent == null || player == null) yield break;

        agent.isStopped = false;
        agent.SetDestination(player.position);

        float stopDistance = 2f;

        // freeze player
        if (playerController != null)
        {
            playerController.SetDialogueActive(true);
        }

        while (Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            // walk towards player
            agent.SetDestination(player.position);

            Vector3 direction = (player.position - transform.position);
            direction.y = 0f;

            // look towards player when walking 
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // stop movement
        agent.isStopped = true;
        agent.ResetPath();

        LookAtPlayer();

        yield return new WaitForSeconds(0.3f);

        Debug.Log("NPC reached player, attempting to start dialogue");

        TryStartDialogue();
    }

    public void OnPlayerInteraction(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        Irene irene = GetComponent<Irene>();
        if (irene != null && irene.IsFollowing)
        {
            Debug.Log("Irene Cannot talk to Irene while she is following");
            return;
        }

        TryStartDialogue();
    }

    // start the dialogue interaction
    private void TryStartDialogue()
    {

        NewDialogueData selectedDialogue = SelectDialogue();

        if (selectedDialogue == null)
        {
            return;
        }

        // stop the npc from wandering if they are a wondering NPC
        npcWander?.StopWandering();

        if (playerMovePoint != null && player != null)
        {
            StartCoroutine(MovePlayerToPosition());
        }

        FaceTarget(player, transform);

        isLookingAtPlayer = true;

        NewDialogueManager.Instance.StartDialogue(selectedDialogue, this);

        hasTalked = true;
    }

    // make the player look at the target
    private void FaceTarget(Transform from, Transform to)
    {
        Vector3 direction = to.position - from.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        from.rotation = lookRotation;
    }

    // select the dialogue intended for the interaction
    private NewDialogueData SelectDialogue()
    {
        bool allCompleted = true;

        // search the objective manager to check if objectives that are conncected to the NPC were completed or not
        foreach (string id in objectiveIDsYouCareAbout)
        {
            if (!ObjectiveManager.Instance.IsObjectiveCompleted(id))
            {
                allCompleted = false;
                break;
            }
        }

        // dialogue selected if the player has completed all the objectives connected to the NPC
        if (allCompleted && taskCompleteDialogueFile != null && hasTalked)
        {
            return taskCompleteDialogueFile;
        }

        // dialogue selected if the player has an active task connected to the NPC
        foreach (string id in objectiveIDsYouCareAbout)
        {
            if (ObjectiveManager.Instance.IsObjectiveActive(id))
            {
                if (taskActiveDialogueFile != null)
                {
                    return taskActiveDialogueFile;
                }
            }
        }

        // dialogue selected if the player has talked to the NPC and has no task active or complete connected to the NPC
        if (hasTalked && talkedDialogueFile != null)
        {
            return talkedDialogueFile;
        }

        // dialogue selected for firts interaction
        return startDialogueFile;
    }

    // when the dialogue interaction is complete
    public void OnDialogueComplete()
    {

        isLookingAtPlayer = false;

        if (linkedObjective != null)
        {
            if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
            {
                ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            }
        }

        // have npc resume wandering if they are a wondering npc
        npcWander?.ResumeWandering();
    }

    public void GiveReward()
    {
        if (rewardGiven) return;

        if (RewardItem == null) return;

        Inventory inventory = FindAnyObjectByType<Inventory>();

        if (inventory != null)
        {
            inventory.AddItem(RewardItem);
            rewardGiven = true;
        }
        else
        {
            Debug.LogError("No inventory found to give reward");
        }
    }

    private IEnumerator MovePlayerToPosition()
    {
        while (Vector3.Distance(player.position, playerMovePoint.position) > 0.05f)
        {
            player.position = Vector3.MoveTowards(player.position, playerMovePoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        LookAtPlayer();
    }

    private void Update()
    {

        if (isLookingAtPlayer && !IsMediation)
        {
            LookAtPlayer();
        }

        CheckNPCInitiatedDialogue();
    }

    // rotate the NPC to look at the player when interacting with the NPC
    private void LookAtPlayer()
    {
        if (player == null) return;

        isTalking = true;
        SafeSetBool("isTalking", isTalking);
        talkRoutine ??= StartCoroutine(TalkAnimationCycle());

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }

    public void StopLookingAtPlayer()
    {
        isTalking = false;
        SafeSetBool("isTalking", isTalking);
        SafeSetBool("Talk1", isTalking);
        SafeSetBool("Talk2", isTalking);

        if (talkRoutine != null)
        {
            StopCoroutine(talkRoutine);
            talkRoutine = null;
        }

        StopPlayerTalking();

        isLookingAtPlayer = false;
    }

    private void StartPlayerTalking()
    {
        if (playerAnimator == null)
            return;
        Debug.Log("Started Talking");
        playerAnimator.SetBool("isTalking", true);
        if (chimeActive)
        {
            chimeAnimator.SetBool("isTalking", true);
        }

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
        if (chimeActive)
        {
            chimeAnimator.SetBool("isTalking", false);
        }


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
        if (animator == null)
        {
            return;
        }

        if (!HasParameter(parameter))
        {
            Debug.LogWarning($"Animator missing parameter: {parameter}", animator);
            return;
        }

        animator.SetBool(parameter, value);

        /*
        if (animator != null)
            animator.SetBool(parameter, value);
        */
    }

    private bool HasParameter(string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    // trigger dialogue interaction for spawn and story trigger types
    private void OnTriggerEnter(Collider other)
    {
        // if trigger type is NPC collision is not detected
        if (triggerType == DialogueTriggerType.NPC || other == null) return;

        // only detect if the player has collided with the trigger
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // only try to start dialogue if the trigger has not talked already
        if (!hasTalked && startDialogueFile != null)
        {
            TryStartDialogue();
        }
    }
}
