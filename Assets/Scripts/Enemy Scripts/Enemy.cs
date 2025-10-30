using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range; // Area around the center point
    public Transform centrePoint; // Center Point of designated area

    public float waitTime = 2f;
    private bool isWaiting = false;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PickNewDestination();
    }

    void Update()
    {
        // If close to destination and not already waiting, start waiting
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitBeforeNextMove());
        }
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
