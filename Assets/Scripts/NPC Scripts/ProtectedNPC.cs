using UnityEngine;
using UnityEngine.AI;

public class ProtectedNPC : MonoBehaviour
{
    public NavMeshAgent agent;
    public TraversablePoint[] CheckPoints;
    public int point;

    [Header("Animation Settings")]
    public Animator animator;

    void Start()
    {
        agent.SetDestination(CheckPoints[point].transform.position);
    }

    void Update()
    {
        if (animator != null)
        {
            bool isMoving = agent.velocity.sqrMagnitude > 0.05f;

            animator.SetBool("isWalking", isMoving);
            animator.SetBool("isIdle", !isMoving);

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
                    if(CheckPoints[point+1].isTraversable)

                    
                    point++;
                    agent.SetDestination(CheckPoints[point].transform.position);
                }      
        }
    }
}