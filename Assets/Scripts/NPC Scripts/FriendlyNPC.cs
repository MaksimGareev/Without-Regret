using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FriendlyNPC : MonoBehaviour
{

    public NavMeshAgent agent;
    public Transform centrePoint;   // Center for wandering
    public float range = 5f;        // How far NPC wanders
    public float waitTime = 2f;
    private bool isWaiting = false;

    void Start()
    {
        // NavMesh setup
        agent = GetComponent<NavMeshAgent>();
        PickNewDestination();
    }
    void Update()
    {
        // NPC wandering (only if not in dialogue)
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !PlayerController.DialogueActive)
        {
            StartCoroutine(WaitBeforeNextMove());
        }
    }

    // Wait before picking next wandering destination
    IEnumerator WaitBeforeNextMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        PickNewDestination();
        isWaiting = false;
    }

    // Pick a random NavMesh point to wander to
    void PickNewDestination()
    {
        Vector3 point;
        if (RandomPoint(centrePoint.position, range, out point))
        {
            agent.SetDestination(point);
        }
    }

    // Random point on NavMesh
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}
