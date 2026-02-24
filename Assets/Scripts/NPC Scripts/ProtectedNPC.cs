using UnityEngine;
using UnityEngine.AI;

public class ProtectedNPC : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] CheckPoints;
    public int point;

    void Start()
    {
        agent.SetDestination(CheckPoints[point].position);
    }

    void Update()
    {
        // If agent reached its destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            point++;

            if (point >= CheckPoints.Length)
                point = 0;

            agent.SetDestination(CheckPoints[point].position);
        }
    }
}