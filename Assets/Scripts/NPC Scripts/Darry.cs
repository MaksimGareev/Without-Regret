using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Darry : MonoBehaviour
{
    // Movement
    public NavMeshAgent agent;
    public Transform[] targets;
    private int currentIndex = 0;
    [HideInInspector] public Transform currentTarget;
    private Coroutine waitAfterBake;

    // movemnet after dialogue
    public float Speed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates

    public Transform targetSpot;
    public Transform lookAtTarget;
    public bool isTraveling;
    public bool arrived = false;
    public float stopDistance = 0.5f;

    private float updateTimer = 0f;
    public float updateRate = 0.2f;

    // objectives
    [SerializeField] ObjectiveData linkedHouseObjective;
    [SerializeField] ObjectiveData linkedNeighborhoodObjective;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targets.Length > 0)
        {
            currentIndex = 0;
            currentTarget = targets[currentIndex];
            agent.SetDestination(targets[currentIndex].position);
        }
        else
        {
            currentTarget = null;
            Debug.LogWarning("No targets assigned to ChasingEnemy!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // stop enemy when dialogue is active
        if (DialogueManager.DialogueIsActive)
        {
            agent.isStopped = true;
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            if (currentIndex < targets.Length && targets[currentIndex] != null)
            {
                agent.SetDestination(targets[currentIndex].position);
            }
            updateTimer = updateRate;
        }


        // Go to next target after reaching current target
        if (!agent.pathPending)
        {
            if (agent.remainingDistance != Mathf.Infinity &&
                agent.remainingDistance <= agent.stoppingDistance &&
                agent.velocity.sqrMagnitude < 0.1f)
            {
                GoToNextTarget();
            }
        }

        /*
        if (!agent.pathPending && agent.remainingDistance < 0.5f && agent != null)
        {
            agent.SetDestination(targets.position);
        }
        if (isTraveling)
        {
            TravelToTarget();
        }
        */
    }

    public void StartTravel()
    {
        //IsFollowing = false;
        isTraveling = true;
        Debug.Log("Darry is now traveling to her destination");
    }

    public void TravelToTarget()
    {
        if (targetSpot == null)
        {
            return;
        }

        Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        // Movement
        //transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, Speed * Time.deltaTime);
        //agent.destination = targetSpot.position;
        agent.SetDestination(currentTarget.position);

        // Rotate towards target
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }

        // Stop when close to target destination
        if (Vector3.Distance(transform.position, targetSpot.position) < stopDistance)
        {
            isTraveling = false;
            arrived = true;
            Debug.Log("Irene reached the destination.");
        }
    }

    void GoToNextTarget()
    {
        //Debug.Log("Going to next point");
        StopWaitCoroutine();
        // ReachedNPC = false;

        /*  // Destroy NPCs or objects if needed
          if (targets[currentIndex] != null && targets[currentIndex].CompareTag("protectedNPC") || targets[currentIndex].CompareTag("Darry"))
          {
              Debug.Log("Enemy reached NPC!");
              Destroy(targets[currentIndex].gameObject, 0.1f);
          }*/

        // Move to next waypoint
        currentIndex++;

        waitAfterBake = StartCoroutine(waitForNavmesh()); //Waits for navmesh to be baked before moving
        if (currentIndex >= targets.Length)
        {
            //Debug.Log("Darry reached final target!");
            currentTarget = null;       // <--- set to null when no more targets
            agent.isStopped = true;     // stop the NavMeshAgent

            StopWaitCoroutine();

            return; // Stop here, no more targets
        }

        currentTarget = targets[currentIndex];
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            StopWaitCoroutine();

            this.gameObject.SetActive(false);
            //ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            Debug.Log("Darry has reached the door.");
        }

        if (other.CompareTag("Finish"))
        {
            StopWaitCoroutine();

            ObjectiveManager.Instance.AddProgress(linkedNeighborhoodObjective.objectiveID, 1);
            Debug.Log("Darry has made it to the end.");
        }
    }

    IEnumerator waitForNavmesh()
    {
        yield return new WaitForSeconds(1.0f);
    }
    private void StopWaitCoroutine() //stops existing WaitForNavmesh coroutine
    {
        if (waitAfterBake != null)
        {
            StopCoroutine(waitAfterBake);
            waitAfterBake = null;
        }
    }
}
