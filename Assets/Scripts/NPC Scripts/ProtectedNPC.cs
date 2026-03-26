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
            if(point+1 < CheckPoints.Length) //checks to see if next point doesn't exist
                {
                    if(CheckPoints[point+1].isTraversable)
                
                    point++;
                    agent.SetDestination(CheckPoints[point].transform.position);
                }      
        }
    }
}