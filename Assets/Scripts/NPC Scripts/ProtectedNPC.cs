using UnityEngine;
using UnityEngine.AI;

public class ProtectedNPC : MonoBehaviour
{
    public NavMeshAgent agent;
    public TraversablePoint[] CheckPoints;
    public int point;

    [Header("Animation Settings")]
    public Animator animator;
    [SerializeField] private bool isScared = false;

    private bool wasOnLink; //Check to not replay Jump trigger

    void Start()
    {
        if (isScared)
        {
            animator.SetBool("isScared", true);
        }
        agent.SetDestination(CheckPoints[point].transform.position);
    }

    void Update()
    {
        bool linkJump = agent.isOnOffMeshLink; //Bool that detects if NPC is jumping across a Navmesh Link

        if (linkJump && !wasOnLink)
        {
            if (animator != null)
            {
                animator.SetTrigger("Jump");
                animator.SetBool("isWalking", false);
                animator.SetBool("isIdle", false);

            }
        }

        wasOnLink = linkJump;
        if (animator != null)
        {
            if (!agent.isOnOffMeshLink) //Dont account for movement animationss if NPC is on a navmesh link
            {
                bool isMoving = agent.velocity.sqrMagnitude > 0.05f && !agent.isOnOffMeshLink;

                animator.SetBool("isWalking", isMoving);
                animator.SetBool("isIdle", !isMoving);
            }
        }

        if (NewDialogueManager.Instance.DialogueIsActive)
        {
            agent.isStopped = true;
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        // If agent reached its destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if(point+1 < CheckPoints.Length) //checks to see if next point doesn't exist
            {
                if (CheckPoints[point + 1].isTraversable)
                {
                    point++;
                    agent.SetDestination(CheckPoints[point].transform.position);
                }
            }      
        }
    }
}