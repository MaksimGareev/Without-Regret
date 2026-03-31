using UnityEngine;
using UnityEngine.AI;

public class Barry : MonoBehaviour
{
    public float Speed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates
    public NewDialogueTrigger dialogueTrigger;
    public Transform targetSpot;
    public Transform lookAtTarget;
    public bool isTraveling = false;
    public bool arrived = false;
    public float stopDistance = 0.5f;
    public NavMeshAgent agent;

    public Animator animator;

    public string npcName = "Barry";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // agent.updateRotation = false;

        if (!animator)
        {
            Debug.LogError($"{this.name} has no animator assigned to the Barry script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTraveling)
        {
            TravelToTarget();
            bool isMoving = agent.velocity.sqrMagnitude > 0.05f;
            
            if (animator)
            {
                animator.SetBool("isWalking", isMoving);
                animator.SetBool("isIdle", !isMoving);
            }
        }
        /*else if (arrived && lookAtTarget != null)
        {
            LookAtObject();
        }*/
    }
    public void StartTravel()
    {
        if (animator)
        {
            animator.SetBool("isTalking", false);
        }
        
        //IsFollowing = false;
        isTraveling = true;
        arrived = false;
        if (dialogueTrigger != null)
        {
            dialogueTrigger.isLookingAtPlayer = false;
        }

        agent.SetDestination(targetSpot.position);
        Debug.Log("Barry is now traveling to her destination");
    }

    public void TravelToTarget()
    {
        if (targetSpot == null)
        {
            Debug.Log("There is no target for Barry to go to");
            return;
        }

        Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        // Movement
        //transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, Speed * Time.deltaTime);
        /*
        if (!agent.pathPending && agent.remainingDistance < 0.5f && agent != null)
        {
            agent.SetDestination(targetSpot.position);
        }
        */
        // Rotate towards target
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
            Debug.Log("Barry reached the destination.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            this.gameObject.SetActive(false);
            //ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            Debug.Log("Barry has reached the door.");
        }
    }

   public bool NPCNameMatches(string name)
    {
        return string.Equals(npcName, name, System.StringComparison.OrdinalIgnoreCase);
    }
}
