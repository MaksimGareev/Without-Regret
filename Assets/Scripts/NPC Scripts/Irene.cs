using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Irene : MonoBehaviour
{
    public Animator animator;
    [HideInInspector] public Transform player;            // the player to follow
    public string npcName = "Irene";    // string data of npc name
    public float FollowDistance = 2f;   // how far behind the player
    public float FollowSpeed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates
    public bool IsFollowing = false;
    private bool isMoving = false; //detects whether or not Irene is moving for animator purposes
    private Vector3 lastPosition;

    public DialogueTrigger dialogueTrigger; // dialogue trigger script reference

    public Transform targetSpot;
    public Transform GoBackHomeSpot;
    public Transform lookAtTarget;
    private NavMeshAgent agent;
    public bool isTraveling;
    public bool arrived = false;
    public bool CanFollowPlayer = true;
    public float stopDistance = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = FollowDistance;
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        movementAnimation();

        if (IsFollowing == true)
        {
            Follow();

            // disable dialogue trigger when following
            if (dialogueTrigger != null && dialogueTrigger.enabled)
            {
                dialogueTrigger.enabled = false;

                Collider col = dialogueTrigger.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
                Debug.Log("Irene's dialogue trigger has been deactivated.");
            }
        }
        else if (isTraveling)
        {
            TravelToTarget();
        }
        else if (arrived && lookAtTarget != null)
        {
            LookAtObject();
        }
    }

    public void Follow()
    {
        animator.SetBool("isTalking", false);
        if (dialogueTrigger != null)
        {
            dialogueTrigger.StopLookingAtPlayer();
        }
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); //calculates distance from the player as NPC is following

        if (distanceToPlayer <= FollowDistance + 1.5f) //stops the NPC from following the player when they are too close
        {
            agent.ResetPath();
            return;
        }

        agent.SetDestination(player.position);

        Vector3 LookDirection = player.position - transform.position;
        LookDirection.y = 0f;

        if (LookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(LookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    public void TravelToTarget()
    {
        animator.SetBool("isTalking", false);
        if (dialogueTrigger != null)
        {
            dialogueTrigger.StopLookingAtPlayer();
        }
        if (targetSpot == null || agent == null)
        {
            Debug.Log("There is no target for Irene to go to");
            return;
        }

        // Movement
        //transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, FollowSpeed * Time.deltaTime);
        if (!agent.hasPath)//(!agent.pathPending && agent.remainingDistance < 0.5f && agent != null)
        {
            agent.SetDestination(targetSpot.position);
        }

        // Rotate towards target
        Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }

        // Stop when close to target destination
        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            isTraveling = false;
            arrived = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            ReactivateDialogue(); ; // enable dialogue trigger upon arrival
            Debug.Log("Irene reached the destination.");
        }
    }

    private void ReactivateDialogue()
    {
        if (dialogueTrigger == null) return;

        dialogueTrigger.enabled = true;

        Collider col = dialogueTrigger.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        Debug.Log("Irene's dialogue trigger has been reactivated");
    }

    public void StartTravel()
    {
        CanFollowPlayer = false;
        IsFollowing = false;
        isTraveling = true;
        dialogueTrigger.isLookingAtPlayer = false;
        arrived = false;
        Debug.Log("Irene is now traveling to her destination");
    }

    public void LookAtObject()
    {
        Vector3 direction = lookAtTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            this.gameObject.SetActive(false);
            //ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            Debug.Log("Irene has reached the door.");
        }
    }

    public bool NPCNameMatches(string name)
    {
        return string.Equals(npcName, name, System.StringComparison.OrdinalIgnoreCase);
    }

    public void movementAnimation()
    {
        if (arrived)
        {
            isMoving = false;
        }
        isMoving = agent.velocity.sqrMagnitude > 0.05f;

        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isIdle", !isMoving);

        //lastPosition = transform.position;
    }
}
