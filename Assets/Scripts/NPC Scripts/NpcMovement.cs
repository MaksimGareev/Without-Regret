using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NpcMovement : MonoBehaviour
{
    // Wander Area
    public Transform centerPoint;
    public float wanderRadius = 5f;

    // Movement
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.5f;

    // Pause Settings
    public float minPauseTime = 1.5f;
    public float maxPauseTime = 3.5f;

    private NavMeshAgent agent;
    private bool isWandering;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        isWandering = true;

        while (isWandering)
        {
            Vector3 targetPoint = GetRandomPointInRadius();

            if (targetPoint != Vector3.zero)
            {
                agent.SetDestination(targetPoint);
            }

            // Wait until NPC reaches destination
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }

            // Pause at destination
            float pauseTime = Random.Range(minPauseTime, maxPauseTime);
            yield return new WaitForSeconds(pauseTime);
        }
    }

    private Vector3 GetRandomPointInRadius()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += centerPoint != null ? centerPoint.position : transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero;
    }

    public void StopWandering()
    {
        isWandering = false;
        agent.ResetPath();
    }

    public void ResumeWandering()
    {
        if (!isWandering)
        {
            StartCoroutine(WanderRoutine());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
