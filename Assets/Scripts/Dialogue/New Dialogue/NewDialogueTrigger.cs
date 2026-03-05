using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewDialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Dialogue Files")]
    public NewDialogueData startDialogueFile;
    public NewDialogueData talkedDialogueFile;
    public NewDialogueData taskActiveDialogueFile;
    public NewDialogueData taskCompleteDialogueFile;

    [Header("Objectives")]
    public List<string> objectiveIDsYouCareAbout = new List<string>();
    public ObjectiveData linkedObjective;

    [Header("Camera")]
    public bool focusCameraOnTrigger = true;

    [Header("Interaction")]
    public float interactionPriority => 10f;
    public InteractType interactType => InteractType.Dialogue;

    [Header("Movement")]
    [SerializeField] private Transform playerMovePoint;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lookSpeed = 6f;


    private bool hasTalked;
    private bool isLookingAtPlayer;
    private Transform player;

    public GameObject enemy;

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

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Debug.Log("Implements IInteractable: " + (this is IInteractable));
    }

    public bool CanInteract(GameObject interactor)
    {
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

    public void OnPlayerInteraction(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        TryStartDialogue();
    }

    private void TryStartDialogue()
    {
        NewDialogueData selectedDialogue = SelectDialogue();

        if (selectedDialogue == null)
        {
            return;
        }

        //npcMovement?.StopWandering();

        if (playerMovePoint != null && player != null)
        {
            StartCoroutine(MovePlayerToPosition());
        }

        isLookingAtPlayer = true;

        NewDialogueManager.Instance.StartDialogue(selectedDialogue, this);

        hasTalked = true;
    }

    private NewDialogueData SelectDialogue()
    {
        bool allCompleted = true;

        foreach (string id in objectiveIDsYouCareAbout)
        {
            if (!ObjectiveManager.Instance.IsObjectiveCompleted(id))
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted && taskCompleteDialogueFile != null && hasTalked)
        {
            return taskCompleteDialogueFile;
        }

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

        if (hasTalked && talkedDialogueFile != null)
        {
            return talkedDialogueFile;
        }

        return startDialogueFile;
    }

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

        //npcMovement?.ResumeWandering();
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
        if (isLookingAtPlayer && player != null)
        {
            LookAtPlayer();
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerType == DialogueTriggerType.NPC || other == null) return;

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!hasTalked && startDialogueFile != null)
        {
            TryStartDialogue();
        }
    }
}
