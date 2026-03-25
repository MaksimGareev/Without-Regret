using UnityEngine;
using UnityEngine.AI;

public class ProtectedNPC : MonoBehaviour
{
    public NavMeshAgent agent;
    public TraversablePoint[] CheckPoints;
    public int point;

    void Start()
    {
        agent.SetDestination(CheckPoints[point].transform.position);
    }

    void Update()
    {
        
        // If agent reached its destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if(CheckPoints[point+1].isTraversable)
                if(point >= CheckPoints.Length) //checks to see if there are no more points to go to
                {
                    return;
                }
                else
                {
                    point++;
                    agent.SetDestination(CheckPoints[point].transform.position);
                }
                
        }
    }
}