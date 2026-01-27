using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingEnemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range; // Area around the center point
    public Transform centrePoint; // Center Point of designated area

    public float waitTime = 2f;
    private bool isWaiting = false;

    // Player morality effects
    public float baseSpeed = 3.5f;
    public float minSpeed = 1.5f;
    public float maxSpeed = 6.5f;
    public float moralitySpeedMultiplier = 0.15f;
    public DialogueManager playerMorality;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PickNewDestination();
    }

    void Update()
    {
        UpdateSpeedFromMorality();//this check will keep happening every frame. perhaps we could change it to happen only once upon seeing the player? It might not affect to much in terms of performance now, but could be an issue if we have a lot of enemies in a level.

        // If close to destination and not already waiting, start waiting
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitBeforeNextMove());
        }
    }

    public void UpdateSpeedFromMorality()
    {
        if (playerMorality == null) return;

        int morality = playerMorality.playerMorality;

        float speedOffset = -morality * moralitySpeedMultiplier;

        float newSpeed = baseSpeed + speedOffset;

        agent.speed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
    }

    // Timer for NPC to pick next random point
    IEnumerator WaitBeforeNextMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime); // Wait at the point
        PickNewDestination(); // Pick next destination
        isWaiting = false;
    }

    void PickNewDestination()
    {
        Vector3 point;
        if (RandomPoint(centrePoint.position, range, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); // Show next point
            agent.SetDestination(point); // Move NPC to point
        }
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; // Picking random point within range
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            
            if (showDebugLogs)
            {
                Debug.Log("Random Point: " + result);
            }

            return true;
        }

        result = Vector3.zero;
        // Debug.Log("search for point failed");
        return false;

    }
}
