using UnityEngine;
using UnityEngine.AI;

public class Darry : MonoBehaviour
{
    // Movement
    public NavMeshAgent agent;
    public Transform target;

    // movemnet after dialogue
    public float Speed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates

    public Transform targetSpot;
    public Transform lookAtTarget;
    public bool isTraveling;
    public bool arrived = false;
    public float stopDistance = 0.5f;

    // objectives
    [SerializeField] ObjectiveData linkedHouseObjective;
    [SerializeField] ObjectiveData linkedNeighborhoodObjective;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

        if (!agent.pathPending && agent.remainingDistance < 0.5f && agent != null)
        {
            agent.SetDestination(target.position);
        }
        if (isTraveling)
        {
            TravelToTarget();
        }
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
        transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, Speed * Time.deltaTime);

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            this.gameObject.SetActive(false);
            //ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            Debug.Log("Darry has reached the door.");
        }

        if (other.CompareTag("Finish"))
        {
            ObjectiveManager.Instance.AddProgress(linkedNeighborhoodObjective.objectiveID, 1);
            Debug.Log("Darry has made it to the end.");
        }
    }
}
